<?php
    //Infinityfree hosting

    //Ustawienie strefy czasowej na Polską, żeby mieć dobry czas kiedy wykonany był test
    date_default_timezone_set('Europe/Warsaw');

    $host = "sql201.infinityfree.com";
    $dbname = "if0_39707398_mgr";
    $username = "if0_39707398";
    $password = "Waligora63";

    // Nazwa pliku CSV
    $csvFile = __DIR__ . "/wyniki_testu_db.csv";

    try {
        $pdo = new PDO("mysql:host=$host;dbname=$dbname;charset=utf8", $username, $password);
        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        //Usuwamy tabelę jeżeli istnieje
        $pdo->exec("DROP TABLE IF EXISTS performance_test");

        //Tworzenie tabeli. Klucz główny + text + int + float + decimal + date żeby zasymulować faktyczne dane w bazie danych
        $pdo->exec("CREATE TABLE IF NOT EXISTS performance_test (
            id INT AUTO_INCREMENT PRIMARY KEY,
            text_val TEXT NOT NULL,
            int_val INT NOT NULL,
            float_val FLOAT NOT NULL,
            dec_val DECIMAL(10,2) NOT NULL,
            date_val DATE NOT NULL
        )");

        $allResults = [];
        $iterations = 5;

        for ($test = 1; $test <= $iterations; $test++) {
            $results = [];

            //INSERT
            $insertStart = microtime(true);
            $stmt = $pdo->prepare("INSERT INTO performance_test (text_val, int_val, float_val, dec_val, date_val) 
                                VALUES (:t, :i, :f, :d, :dt)");
            for ($i = 0; $i < 5000; $i++) {
                $stmt->execute([
                    't' => str_repeat("test", 50),
                    'i' => rand(1, 100000),
                    'f' => mt_rand() / mt_getrandmax() * 1000,
                    'd' => mt_rand(1000, 1000000) / 100,
                    'dt' => date("Y-m-d", strtotime("+$i days"))
                ]);
            }
            $insertTime = round((microtime(true) - $insertStart) * 1000, 2);
            $results['INSERT'] = $insertTime;
            
            //SELECT
            $selectStart = microtime(true);
            $stmt = $pdo->query("SELECT * FROM performance_test");
            $rows = $stmt->fetchAll();
            $selectTime = round((microtime(true) - $selectStart) * 1000, 2);
            $results['SELECT'] = $selectTime;

            //UPDATE
            $updateStart = microtime(true);
            $stmt = $pdo->prepare("UPDATE performance_test 
                                SET text_val=:t, int_val=:i, float_val=:f, dec_val=:d, date_val=:dt 
                                WHERE id=:id");
            for ($i = 1; $i <= 5000; $i++) {
                $stmt->execute([
                    't' => str_repeat("upd", 50),
                    'i' => rand(1, 100000),
                    'f' => mt_rand() / mt_getrandmax() * 1000,
                    'd' => mt_rand(1000, 1000000) / 100,
                    'dt' => date("Y-m-d", strtotime("-$i days")),
                    'id' => $i
                ]);
            }
            $updateTime = round((microtime(true) - $updateStart) * 1000, 2);
            $results['UPDATE'] = $updateTime;

            //DELETE
            $deleteStart = microtime(true);
            $pdo->exec("DELETE FROM performance_test");
            $deleteTime = round((microtime(true) - $deleteStart) * 1000, 2);
            $results['DELETE'] = $deleteTime;

            $allResults[] = $results;
        }

        //Liczenie wartości statystycznych takich jak średnia czasów, min, max, mediana, odchylenie standardowe, współczynnik stabilności
        $summary = [];
        foreach (['INSERT', 'SELECT', 'UPDATE', 'DELETE'] as $op) {
            //Wyciągamy czasy dla operacji i wykonujemy na tych czasach operacje (sortowanie, liczenie średniej itd.)
            $times = array_column($allResults, $op);
            sort($times);
            $count = count($times);
            $avg = array_sum($times) / $count;
            $min = $times[0];
            $max = $times[$count - 1];
            $mid = floor($count / 2);
            $median = ($count % 2 == 0) ? ($times[$mid - 1] + $times[$mid]) / 2 : $times[$mid];
            $variance = 0;
            foreach ($times as $v) $variance += pow($v - $avg, 2);
            $stdev = sqrt($variance / $count);
            //Współczynnik stabilności
            $stability = $max > 0 ? $max / $min : 0;
            //Tworzenie tablicy z wyliczonymi wartościami statystycznymi
            $summary[$op] = [
                //średnia
                'avg' => round($avg, 2),
                //min
                'min' => round($min, 2),
                //max
                'max' => round($max, 2),
                //mediana
                'median' => round($median, 2),
                //odchylenie stadardowe
                'stdev' => round($stdev, 2),
                //współczynnik stabilności
                'stability' => round($stability, 2)
            ];
        }

        //Wyświetlanie wyników: czasy (tablica $allResults) oraz wartości statystyczne (tablica $summary)
        echo "<h2>Wyniki testu wydajności bazy danych (".$iterations." iteracji)</h2>";
        echo "<table border='1' cellpadding='8' cellspacing='0'>
            <tr>
                <th style='text-align:center'>Test</th>
                <th style='text-align:center'>INSERT</th>
                <th style='text-align:center'>SELECT</th>
                <th style='text-align:center'>UPDATE</th>
                <th style='text-align:center'>DELETE</th>
            </tr>";

        foreach ($allResults as $index => $res) {
            echo "<tr><td style='text-align:center'>Test " . ($index + 1) . "</td>";
            foreach (['INSERT', 'SELECT', 'UPDATE', 'DELETE'] as $op) {
                echo "<td style='text-align:center'>{$res[$op]} ms</td>";
            }
            echo "</tr>";
        }

        foreach (['avg'=>'AVG','min'=>'MIN','max'=>'MAX','median'=>'MEDIAN','stdev'=>'STDEV','stability'=>'STABILITY'] as $k=>$label) {
            echo "<tr><th style='text-align:center'>$label</th>";
            foreach (['INSERT', 'SELECT', 'UPDATE', 'DELETE'] as $op) {
                if($label === 'STABILITY') {
                    echo "<th style='text-align:center'>{$summary[$op][$k]}</th>";
                } else {
                    echo "<th style='text-align:center'>{$summary[$op][$k]} ms</th>";
                }
            }
            echo "</tr>";
        }
        echo "</table>";

        //Zapisanie wyników do pliku (godzina w której nastąpił test, nazwa hostingu, czasy, wartości statystyczne)
        $newFile = !file_exists($csvFile);
        $fp = fopen($csvFile, 'a');
        if ($fp) {
            if ($newFile) {
                fwrite($fp, "\xEF\xBB\xBF");
                fwrite($fp, "sep=;\n");
                fputcsv($fp, ['Timestamp','Hosting','Test','INSERT','SELECT','UPDATE','DELETE'], ';');
            }
            $timestamp = date("Y-m-d H:i:s");
            $hostingName = "infinityfree";

            foreach ($allResults as $index => $res) {
                $values = array_map(fn($v) => str_replace('.', ',', $v), $res);
                fputcsv($fp, array_merge([$timestamp, $hostingName, "Test " . ($index + 1)], $values), ';');
            }

            fclose($fp);
            echo "<p>Wyniki zapisane do pliku: wyniki_testu_db.csv</p>";
        } else {
            echo "<p style='color:red;'>Błąd zapisu do pliku CSV!</p>";
        }

    } catch (PDOException $e) {
        echo "<p>Błąd połączenia z bazą danych: " . $e->getMessage() . "</p>";
    }
?>