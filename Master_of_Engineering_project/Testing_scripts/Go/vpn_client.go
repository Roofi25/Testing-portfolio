package main

import (
	"fmt"
	"io"
	"net/http"
	"os"
	"strings"
	"time"
)

const (
	iterations  = 100
	hostingName = "render"
	csvFile     = "wyniki_testu_vpn.csv"
	base        = "https://rendertesting-wdkv.onrender.com/" 
)

// Funkcja mierząca czas
func measureRequest(url string) float64 {
    // Wyłączone Keep-Alive
    tr := &http.Transport{
        DisableKeepAlives: true,
    }
    client := &http.Client{
        Transport: tr,
        Timeout:   15 * time.Second,
    }

    start := time.Now()
    
    // Tworzenie obiektu żądania
    req, err := http.NewRequest("GET", url, nil)
    if err != nil {
        return 0
    }
    
    req.Header.Set("Connection", "close")
    req.Header.Set("Cache-Control", "no-cache")

    resp, err := client.Do(req)
    if err != nil {
        fmt.Printf("\nBłąd zapytania do %s: %v", url, err)
        return 0
    }
    defer resp.Body.Close()
	
    _, _ = io.Copy(io.Discard, resp.Body)

    return time.Since(start).Seconds() * 1000
}

func main() {
	// Otwieramy plik w trybie dopisywania
	file, err := os.OpenFile(csvFile, os.O_APPEND|os.O_CREATE|os.O_WRONLY, 0644)
	if err != nil {
		fmt.Println("Błąd otwarcia pliku CSV:", err)
		return
	}
	defer file.Close()

	stat, _ := file.Stat()
	if stat.Size() == 0 {
		file.WriteString("\xEF\xBB\xBF") // BOM dla Excela
		file.WriteString("sep=;\n")
		file.WriteString("Timestamp;Hosting;Test;1KB;10KB;100KB\n")
	}

	now := time.Now()
	timestamp := now.Format("2006-01-02 15:04:05")

	for i := 1; i <= iterations; i++ {
		t1 := measureRequest(base + "response_1KB")
		t10 := measureRequest(base + "response_10KB")
		t100 := measureRequest(base + "response_100KB")

		val1 := strings.Replace(fmt.Sprintf("%.2f", t1), ".", ",", 1)
		val10 := strings.Replace(fmt.Sprintf("%.2f", t10), ".", ",", 1)
		val100 := strings.Replace(fmt.Sprintf("%.2f", t100), ".", ",", 1)

		row := fmt.Sprintf("%s;%s;Test %d;%s;%s;%s\n",
			timestamp, hostingName, i, val1, val10, val100)

		file.WriteString(row)
	}
	//pusty wiersz
	file.WriteString(";;;;;\n")
	
	fmt.Println("\nWyniki zapisane do pliku:", csvFile)
}