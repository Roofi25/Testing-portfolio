package main
import "strings"

import (
	"bytes"
	"context"
	"fmt"
	"log"
	"math"
	"math/rand"
	"net/http"
	"os"
	"sort"
	"time"
	"github.com/jackc/pgx/v5"
	"encoding/json"
)

var lastDbResults []map[string]float64
var lastAlgoResults []map[string]interface{}

// Z powodów bezpieczeństwa usunięto prawdziwe dane dostępowe do bazy
const connectionString = "postgres://<username>:<password>@<neon_host_url>/<database_name>";

func formatDate(t time.Time) string {
	loc, _ := time.LoadLocation("Europe/Warsaw")
	return t.In(loc).Format("02.01.2006 15:04")
}

func formatNumberCSV(num float64) string {
    return strings.Replace(fmt.Sprintf("%.2f", num), ".", ",", 1)
}

func indexHandler(w http.ResponseWriter, r *http.Request) {
	html := `
	<!DOCTYPE html>
	<html lang="pl">
	<head>
	<meta charset="UTF-8" />
	<title>Test Wydajności Hostingów</title>
	<style>
		body { font-family: Arial, sans-serif; background-color: #f8f8f8; padding: 20px; text-align: center; }
		a { color: #007bff; font-weight: bold; text-decoration: none; }
	</style>
	</head>
	<body>
	<h1>Witamy na stronie testowej</h1>
	<p><a href="/db_test">Kliknij tutaj, aby uruchomić test bazy danych</a></p>
	<p><a href="/algo_test">Kliknij tutaj, aby uruchomić test algorytmiczny</a></p>
	<p><a href="/knn_test">Kliknij tutaj, aby uruchomić test KNN</a></p>
	</body>
	</html>
	`
	w.Header().Set("Content-Type", "text/html; charset=UTF-8")
	fmt.Fprint(w, html)
}

func dbTestHandler(w http.ResponseWriter, r *http.Request) {
	conn, err := pgx.Connect(context.Background(), connectionString)
	if err != nil {
		http.Error(w, fmt.Sprintf("Błąd połączenia: %v", err), http.StatusInternalServerError)
		return
	}
	defer conn.Close(context.Background())

	_, err = conn.Exec(context.Background(), "DROP TABLE IF EXISTS performance_test")
	if err != nil {
		http.Error(w, fmt.Sprintf("Błąd DROP TABLE: %v", err), http.StatusInternalServerError)
		return
	}

	_, err = conn.Exec(context.Background(), `
	CREATE TABLE performance_test (
		id SERIAL PRIMARY KEY,
		text_val TEXT NOT NULL,
		int_val INT NOT NULL,
		float_val REAL NOT NULL,
		dec_val NUMERIC(10,2) NOT NULL,
		date_val DATE NOT NULL
	)`)
	if err != nil {
		http.Error(w, fmt.Sprintf("Błąd CREATE TABLE: %v", err), http.StatusInternalServerError)
		return
	}

	iteracje := 5
	allResults := make([]map[string]float64, 0, iteracje)

	for t := 0; t < iteracje; t++ {
		wyniki := make(map[string]float64)

		// BATCH INSERT
		insertValues := make([]string, 0, 5000)
		insertParams := make([]interface{}, 0, 5000*5)
		for i := 0; i < 5000; i++ {
			insertParams = append(insertParams,
				repeatString("test", 50),
				rand.Intn(100000),
				rand.Float64()*1000,
				math.Floor(rand.Float64()*1000000)/100,
				time.Now().Add(time.Duration(i)*24*time.Hour),
			)
			base := len(insertParams)
			insertValues = append(insertValues,
				fmt.Sprintf("($%d,$%d,$%d,$%d,$%d)", base-4, base-3, base-2, base-1, base),
			)
		}
		insertStart := time.Now()
		_, err = conn.Exec(context.Background(),
			fmt.Sprintf("INSERT INTO performance_test (text_val,int_val,float_val,dec_val,date_val) VALUES %s",
				joinStrings(insertValues, ",")),
			insertParams...,
		)
		if err != nil {
			http.Error(w, fmt.Sprintf("Błąd INSERT: %v", err), http.StatusInternalServerError)
			return
		}
		wyniki["INSERT"] = time.Since(insertStart).Seconds() * 1000

		// SELECT
		selectStart := time.Now()
		_, err = conn.Exec(context.Background(), "SELECT COUNT(*) FROM performance_test")
		if err != nil {
			http.Error(w, fmt.Sprintf("Błąd SELECT: %v", err), http.StatusInternalServerError)
			return
		}
		wyniki["SELECT"] = time.Since(selectStart).Seconds() * 1000

		// BATCH UPDATE
		updateStart := time.Now()
		batch := &pgx.Batch{}

		for i := 1; i <= 5000; i++ {
			batch.Queue(
				`UPDATE performance_test SET 
					text_val = $2, 
					int_val = $3, 
					float_val = $4, 
					dec_val = $5, 
					date_val = $6 
				WHERE id = $1`,
				i,
				repeatString("upd", 50),
				rand.Intn(100000),
				rand.Float64()*1000,
				math.Floor(rand.Float64()*1000000)/100,
				time.Now().Add(-time.Duration(i)*24*time.Hour),
			)
		}

		br := conn.SendBatch(context.Background(), batch)
		err := br.Close()
		if err != nil {
			http.Error(w, fmt.Sprintf("Błąd UPDATE: %v", err), http.StatusInternalServerError)
			return
		}

		wyniki["UPDATE"] = time.Since(updateStart).Seconds() * 1000

		// DELETE
		deleteStart := time.Now()
		_, _ = conn.Exec(context.Background(), "DELETE FROM performance_test")
		wyniki["DELETE"] = time.Since(deleteStart).Seconds() * 1000

		allResults = append(allResults, wyniki)
	}

	lastDbResults = allResults

	html := fmt.Sprintf(`<h2>Wyniki testu bazy danych (%d iteracji)</h2>
	<table border="1" cellpadding="8" cellspacing="0">
	<tr>
	<th style="text-align:center">Test</th>
	<th style="text-align:center">INSERT</th>
	<th style="text-align:center">SELECT</th>
	<th style="text-align:center">UPDATE</th>
	<th style="text-align:center">DELETE</th>
	</tr>`, iteracje)

	for i, r := range allResults {
		html += fmt.Sprintf(`<tr>
		<td style="text-align:center">Test %d</td>
		<td style="text-align:center">%.2f ms</td>
		<td style="text-align:center">%.2f ms</td>
		<td style="text-align:center">%.2f ms</td>
		<td style="text-align:center">%.2f ms</td>
		</tr>`, i+1, r["INSERT"], r["SELECT"], r["UPDATE"], r["DELETE"])
	}

	type Stats struct {
		avg, min, max, median, stdev, stability float64
	}
	summary := map[string]Stats{}
	for _, op := range []string{"INSERT", "SELECT", "UPDATE", "DELETE"} {
		times := make([]float64, 0, len(allResults))
		for _, r := range allResults {
			times = append(times, r[op])
		}
		sort.Float64s(times)
		count := float64(len(times))
		sum := 0.0
		for _, v := range times {
			sum += v
		}
		avg := sum / count
		min := times[0]
		max := times[len(times)-1]
		median := 0.0
		if int(count)%2 == 0 {
			median = (times[int(count/2-1)] + times[int(count/2)]) / 2
		} else {
			median = times[int(count/2)]
		}
		variance := 0.0
		for _, v := range times {
			variance += (v - avg) * (v - avg)
		}
		variance /= count
		stdev := math.Sqrt(variance)
		stability := max / min
		summary[op] = Stats{avg, min, max, median, stdev, stability}
	}

	summaryOrder := []struct{
		key, label string
	}{
		{"avg", "AVG"},
		{"min", "MIN"},
		{"max", "MAX"},
		{"median", "MEDIAN"},
		{"stdev", "STDEV"},
		{"stability", "STABILITY"},
	}

	for _, s := range summaryOrder {
		html += fmt.Sprintf("<tr><th style='text-align:center'>%s</th>", s.label)
		for _, op := range []string{"INSERT", "SELECT", "UPDATE", "DELETE"} {
			var val float64
			switch s.key {
			case "avg":
				val = summary[op].avg
			case "min":
				val = summary[op].min
			case "max":
				val = summary[op].max
			case "median":
				val = summary[op].median
			case "stdev":
				val = summary[op].stdev
			case "stability":
				val = summary[op].stability
			}
			if s.key == "stability" {
				html += fmt.Sprintf("<th style='text-align:center'>%.2f</th>", val)
			} else {
				html += fmt.Sprintf("<th style='text-align:center'>%.2f ms</th>", val)
			}
		}
		html += "</tr>"
	}

	html += `</table>`
	html += `<p><a href="/download_db_csv" download>Pobierz plik csv z wynikami</a></p>`

	w.Header().Set("Content-Type", "text/html; charset=UTF-8")
	fmt.Fprint(w, html)
}

func downloadDbCsvHandler(w http.ResponseWriter, r *http.Request) {
	if len(lastDbResults) == 0 {
		http.Error(w, "Brak danych — najpierw uruchom /db_test", http.StatusBadRequest)
		return
	}

	var buf bytes.Buffer
	buf.WriteString("\uFEFF")
	buf.WriteString("sep=;\n")
	buf.WriteString("Timestamp;Hosting;Test;INSERT;SELECT;UPDATE;DELETE\n")

	timestamp := formatDate(time.Now())
	hosting := "render"

	for i, r := range lastDbResults {
		buf.WriteString(fmt.Sprintf("%s;%s;Test %d;%s;%s;%s;%s\n",
			timestamp, hosting, i+1,
			formatNumberCSV(r["INSERT"]),
			formatNumberCSV(r["SELECT"]),
			formatNumberCSV(r["UPDATE"]),
			formatNumberCSV(r["DELETE"]),
		))
	}

	w.Header().Set("Content-Type", "text/csv; charset=UTF-8")
	w.Header().Set("Content-Disposition", `attachment; filename="wyniki_testu_db.csv"`)
	w.Write(buf.Bytes())
}

func testSorting(size int) float64 {
	arr := make([]int, size)
	for i := 0; i < size; i++ {
		arr[i] = rand.Intn(1_000_000)
	}
	start := time.Now()
	sort.Ints(arr)
	return time.Since(start).Seconds() * 1000
}

func testPrimes(limit int) float64 {
	start := time.Now()
	sieve := make([]bool, limit+1)
	for i := range sieve {
		sieve[i] = true
	}
	sieve[0], sieve[1] = false, false
	for i := 2; i*i <= limit; i++ {
		if sieve[i] {
			for j := i * i; j <= limit; j += i {
				sieve[j] = false
			}
		}
	}
	return time.Since(start).Seconds() * 1000
}

func testFibonacci(n int) float64 {
	start := time.Now()
	a, b := 0, 1
	for i := 2; i <= n; i++ {
		a, b = b, a+b
	}
	return time.Since(start).Seconds() * 1000
}

func testStringOps(iterations int) float64 {
	start := time.Now()
	var sb strings.Builder
	// Alokacja pamięci
	sb.Grow(1000)
	sb.WriteByte('a')
	for i := 0; i < iterations; i++ {
		sb.WriteByte('b')
		if sb.Len() > 1000 {
			s := sb.String()
			sb.Reset()
			sb.WriteString(s[500:])
		}
	}
	return time.Since(start).Seconds() * 1000
}


func algoTestHandler(w http.ResponseWriter, r *http.Request) {
	iterations := 5
	algorithms := []string{"SORTING", "PRIMES", "FIBONACCI", "STRINGS"}
	allResults := make([]map[string]interface{}, 0, iterations)

	for t := 0; t < iterations; t++ {
		res := map[string]interface{}{
			"timestamp": formatDate(time.Now()),
			"hosting":   "render",
			"SORTING":   testSorting(2_000_000),
			"PRIMES":    testPrimes(2_000_000),
			"FIBONACCI": testFibonacci(30_000_000),
			"STRINGS":   testStringOps(30_000_000),
		}
		allResults = append(allResults, res)
	}

	lastAlgoResults = allResults

	summary := map[string]map[string]float64{}
	for _, alg := range algorithms {
		times := make([]float64, 0, iterations)
		for _, r := range allResults {
			times = append(times, r[alg].(float64))
		}
		sort.Float64s(times)
		count := float64(len(times))
		sum := 0.0
		for _, v := range times {
			sum += v
		}
		avg := sum / count
		min := times[0]
		max := times[len(times)-1]
		median := 0.0
		if int(count)%2 == 0 {
			median = (times[int(count/2-1)] + times[int(count/2)]) / 2
		} else {
			median = times[int(count/2)]
		}
		variance := 0.0
		for _, v := range times {
			variance += (v - avg) * (v - avg)
		}
		variance /= count
		stdev := math.Sqrt(variance)
		stability := max / min
		summary[alg] = map[string]float64{
			"avg": avg, "min": min, "max": max, "median": median, "stdev": stdev, "stability": stability,
		}
	}

	html := fmt.Sprintf(`<h2>Wyniki testu algorytmicznego (%d iteracji)</h2>
	<table border="1" cellpadding="8" cellspacing="0">
	<tr>
	<th style="text-align:center">Test</th>
	<th style="text-align:center">Sortowanie</th>
	<th style="text-align:center">Liczby pierwsze</th>
	<th style="text-align:center">Fibonacci</th>
	<th style="text-align:center">Operacje na stringach</th>
	</tr>`, iterations)

	for i, r := range allResults {
		html += fmt.Sprintf("<tr><td style='text-align:center'>Test %d</td>", i+1)
		for _, alg := range algorithms {
			html += fmt.Sprintf("<td style='text-align:center'>%.2f ms</td>", r[alg].(float64))
		}
		html += "</tr>"
	}

		summaryOrder := []struct{ key, label string }{
		{"avg", "AVG"},
		{"min", "MIN"},
		{"max", "MAX"},
		{"median", "MEDIAN"},
		{"stdev", "STDEV"},
		{"stability", "STABILITY"},
	}

	for _, s := range summaryOrder {
		html += fmt.Sprintf("<tr><th style='text-align:center'>%s</th>", s.label)
		for _, alg := range algorithms {
			val := summary[alg][s.key]
			if s.key == "stability" {
				html += fmt.Sprintf("<th style='text-align:center'>%.2f</th>", val)
			} else {
				html += fmt.Sprintf("<th style='text-align:center'>%.2f ms</th>", val)
			}
		}
		html += "</tr>"
	}

	html += `</table>`
	html += `<p><a href="/download_algo_csv" download>Pobierz plik csv z wynikami</a></p>`

	w.Header().Set("Content-Type", "text/html; charset=UTF-8")
	fmt.Fprint(w, html)
}

func downloadAlgoCsvHandler(w http.ResponseWriter, r *http.Request) {
	if len(lastAlgoResults) == 0 {
		http.Error(w, "Nie ma danych do pobrania. Najpierw przeprowadź test!", http.StatusBadRequest)
		return
	}

	var buf bytes.Buffer
	buf.WriteString("\uFEFF")
	buf.WriteString("sep=;\n")
	buf.WriteString("Timestamp;Hosting;Type;Sorting;Primes;Fibonacci;Strings\n")

	for i, r := range lastAlgoResults {
		buf.WriteString(fmt.Sprintf("%s;%s;Test %d;%s;%s;%s;%s\n",
			r["timestamp"], r["hosting"], i+1,
			formatNumberCSV(r["SORTING"].(float64)),
			formatNumberCSV(r["PRIMES"].(float64)),
			formatNumberCSV(r["FIBONACCI"].(float64)),
			formatNumberCSV(r["STRINGS"].(float64)),
		))
	}

	w.Header().Set("Content-Type", "text/csv; charset=UTF-8")
	w.Header().Set("Content-Disposition", `attachment; filename="wyniki_testu_algo.csv"`)
	w.Write(buf.Bytes())
}

func joinStrings(arr []string, sep string) string {
	var b strings.Builder
	for i, s := range arr {
		if i > 0 {
			b.WriteString(sep)
		}
		b.WriteString(s)
	}
	return b.String()
}

func repeatString(s string, n int) string {
	var b strings.Builder
	for i := 0; i < n; i++ {
		b.WriteString(s)
	}
	return b.String()
}

// KNN
var lastKnnResults []float64

type KnnPoint struct {
    x, y  int
    label int
}

type KnnDistance struct {
    d int
    l int
}

func testKNN(pointsTrain int, pointsTest int, k int) float64 {
    // Stałe ziarno dla identycznych danych
    source := rand.NewSource(12345)
    r := rand.New(source)

    trainingSet := make([]KnnPoint, pointsTrain)
    for i := 0; i < pointsTrain; i++ {
        trainingSet[i] = KnnPoint{
            x:     r.Intn(1001),
            y:     r.Intn(1001),
            label: r.Intn(2),
        }
    }

    testingSet := make([]KnnPoint, pointsTest)
    for i := 0; i < pointsTest; i++ {
        testingSet[i] = KnnPoint{
            x: r.Intn(1001),
            y: r.Intn(1001),
        }
    }

    start := time.Now()

    // Główna pętla klasyfikacji
    for _, p := range testingSet {
        distances := make([]KnnDistance, pointsTrain)
        for j, t := range trainingSet {
            dx := p.x - t.x
            dy := p.y - t.y
            // Odległość euklidesowa (bez pierwiastka)
            distances[j] = KnnDistance{d: dx*dx + dy*dy, l: t.label}
        }

        // Sortowanie dystansów
        sort.Slice(distances, func(i, j int) bool {
            return distances[i].d < distances[j].d
        })

        // Głosowanie k-sąsiadów
        votes := [2]int{0, 0}
        for i := 0; i < k; i++ {
            votes[distances[i].l]++
        }
		// wyniku nie uzywamy, tylko mierzymy czas
        _ = 0
        if votes[1] > votes[0] {
            _ = 1
        }
    }

    return time.Since(start).Seconds() * 1000 // Wynik w ms
}

func knnTestHandler(w http.ResponseWriter, r *http.Request) {
    iterations := 100
    results := make([]float64, iterations)
    for i := 0; i < iterations; i++ {
        results[i] = testKNN(200, 500, 5)
    }
    lastKnnResults = results

    html := `<p><a href="/download_knn_csv" download>Pobierz plik csv z wynikami</a></p>`
    w.Header().Set("Content-Type", "text/html; charset=UTF-8")
    fmt.Fprint(w, html)
}

func downloadKnnCsvHandler(w http.ResponseWriter, r *http.Request) {
    if len(lastKnnResults) == 0 {
        http.Error(w, "Brak danych k-NN — najpierw uruchom /knn_test", http.StatusBadRequest)
        return
    }

    var buf bytes.Buffer
    for _, res := range lastKnnResults {
        buf.WriteString(formatNumberCSV(res) + "\n")
    }

    w.Header().Set("Content-Type", "text/csv; charset=UTF-8")
    w.Header().Set("Content-Disposition", `attachment; filename="wyniki_testu_knn.csv"`)
    w.Write(buf.Bytes())
}

var (
	data1KB   = strings.Repeat("A", 1024)
	data10KB  = strings.Repeat("A", 10240)
	data100KB = strings.Repeat("A", 102400)
)

func response1KBHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "text/plain")
	fmt.Fprint(w, data1KB)
}

func response10KBHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "text/plain")
	fmt.Fprint(w, data10KB)
}

func response100KBHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "text/plain")
	fmt.Fprint(w, data100KB)
}

// Ednpointy dla testów w jmeter
// Strona statyczna
func staticHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "text/plain")
	fmt.Fprint(w, "OK")
}

// JSON - pobranie ok. 3KB danych strukturalnych
func jsonHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")
	data := make([]map[string]interface{}, 50)
	for i := 0; i < 50; i++ {
		data[i] = map[string]interface{}{
			"id":        i,
			"status":    "active",
			"label":     "test_data_point",
			"timestamp": time.Now().UnixMilli(),
		}
	}
	importJson, _ := json.Marshal(data)
	w.Write(importJson)
}

// Wyliczanie w pętli sumy pierwiastków
func computeHandler(w http.ResponseWriter, r *http.Request) {
    sum := 0.0
    for i := 0; i < 100000; i++ {
        sum += math.Sqrt(float64(i))
    }
    w.Header().Set("Content-Type", "text/plain")
    fmt.Fprintf(w, "Result: %.2f", sum)
}

func main() {
	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	http.HandleFunc("/", indexHandler)
	http.HandleFunc("/db_test", dbTestHandler)
	http.HandleFunc("/download_db_csv", downloadDbCsvHandler)
	http.HandleFunc("/algo_test", algoTestHandler)
	http.HandleFunc("/download_algo_csv", downloadAlgoCsvHandler)
	http.HandleFunc("/knn_test", knnTestHandler)
	http.HandleFunc("/download_knn_csv", downloadKnnCsvHandler)

	http.HandleFunc("/response_1KB", response1KBHandler)
    http.HandleFunc("/response_10KB", response10KBHandler)
    http.HandleFunc("/response_100KB", response100KBHandler)

	http.HandleFunc("/static", staticHandler)
	http.HandleFunc("/json", jsonHandler)
	http.HandleFunc("/compute", computeHandler)

	log.Printf("Server running on port %s", port)
	log.Fatal(http.ListenAndServe(":"+port, nil))
}
