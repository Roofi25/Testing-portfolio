<?php
    //InfinityFree hosting

    //Ustawienie strefy czasowej na Polską, żeby mieć dobry czas kiedy wykonany był test
    date_default_timezone_set('Europe/Warsaw');

    // Nazwa pliku CSV
    $csvFile = __DIR__ . "/wyniki_testu_algo.csv";

    //Sortowanie tablicy 2 milionów losowych elementów
    function testSorting($size = 2000000) {
        mt_srand(12345); //stałe ziarno żeby przy każdym uruchomieniu skryptu sortowało tę samą tablicę (z tymi samymi danymi)
        $array = [];
        for ($i = 0; $i < $size; $i++) {
            $array[] = rand();
        }
        $start = microtime(true);
        sort($array);
        return round((microtime(true) - $start) * 1000, 2);
    }


    //Znalezienie wszystkich liczb pierwszych od 0 aż do liczby 2 milionów (limit)
    function testPrimes($limit = 2000000) {
        $start = microtime(true);
        $sieve = array_fill(0, $limit + 1, true);
        $sieve[0] = $sieve[1] = false;
        for ($i = 2; $i * $i <= $limit; $i++) {
            if ($sieve[$i]) {
                for ($j = $i * $i; $j <= $limit; $j += $i) {
                    $sieve[$j] = false;
                }
            }
        }
        return round((microtime(true) - $start) * 1000, 2);
    }

    //Znalezienie pierwszych 30 milionów liczb ciągu fibonacchiego
    function testFibonacci($n = 30000000) {
        $start = microtime(true);
        $fib1 = 0;
        $fib2 = 1;
        for ($i = 2; $i <= $n; $i++) {
            $fib3 = $fib1 + $fib2;
            $fib1 = $fib2;
            $fib2 = $fib3;
        }
        return round((microtime(true) - $start) * 1000, 2);
    }

    //Operacje na ciągu znaków (konkatenacja, wycięcie) - testują jak szybko hosting poradzi sobie z operacjami na ciągu znaków.
    //30 milionów iteracji.
    function testStringOps($iterations = 30000000) {
        $start = microtime(true);
        $str = "a";
        for ($i = 0; $i < $iterations; $i++) {
            $str .= "b";
            if (strlen($str) > 1000) {
                $str = substr($str, 500);
            }
        }
        return round((microtime(true) - $start) * 1000, 2);
    }

    //tablica z wszystkimi rezultatami (wielkość taka jaka jest liczba operacji)
    $allResults = [];
    $iterations = 5;
    $algorithms = ['SORTING', 'PRIMES', 'FIBONACCI', 'STRINGS'];

    // Wykonanie testów tyle ile wynosi wartość zmiennej "$iterations"
    for ($test = 1; $test <= $iterations; $test++) {
        $results = [];
        $results['SORTING'] = testSorting();
        $results['PRIMES'] = testPrimes();
        $results['FIBONACCI'] = testFibonacci();
        $results['STRINGS'] = testStringOps();
        $allResults[] = $results;
    }

    //Liczenie wartości statystycznych takich jak średnia czasów, min, max, mediana, odchylenie standardowe, współczynnik stabilności
    $summary = [];
    foreach (['SORTING', 'PRIMES', 'FIBONACCI', 'STRINGS'] as $alg) {
        //Wyciągamy czasy dla algorytmów i wykonujemy na tych czasach operacje (sortowanie tych czasów, liczenie średniej itd.)
        $times = array_column($allResults, $alg);
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
        $summary[$alg] = [
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

    // Wyświetlenie wyników w tabeli HTML
    echo "<h2>Wyniki testu algorytmicznego ({$iterations} iteracji)</h2>";
    echo "<table border='1' cellpadding='8' cellspacing='0'>
            <tr>
                <th style='text-align:center'>Test</th>
                <th style='text-align:center'>Sortowanie</th>
                <th style='text-align:center'>Liczby pierwsze</th>
                <th style='text-align:center'>Fibonacci</th>
                <th style='text-align:center'>Operacje na stringach</th>
            </tr>";

    foreach ($allResults as $index => $res) {
        echo "<tr><td style='text-align:center'>Test " . ($index + 1) . "</td>";
        foreach ($algorithms as $alg) {
            echo "<td style='text-align:center'>{$res[$alg]} ms</td>";
        }
        echo "</tr>";
    }
    foreach (['avg'=>'AVG','min'=>'MIN','max'=>'MAX','median'=>'MEDIAN','stdev'=>'STDEV','stability'=>'STABILITY'] as $k=>$label) {
        echo "<tr><th style='text-align:center'>$label</th>";
        foreach ($algorithms as $alg) {
            if($label === 'STABILITY')
            {
                echo "<th style='text-align:center'>{$summary[$alg][$k]}</th>";
            }
            else
            {
                echo "<th style='text-align:center'>{$summary[$alg][$k]} ms</th>";
            }
        }
        echo "</tr>";
    }

    echo "</table>";

    // Zapis wyników do pliku CSV (UTF-8 BOM)
    $newFile = !file_exists($csvFile); // sprawdzamy, czy plik jest nowy
    $fp = fopen($csvFile, 'a');
    if ($fp) {
        //Jeżeli to nowy plik to:
        if ($newFile) {
            fwrite($fp, "\xEF\xBB\xBF"); // UTF-8 BOM dla Excela
            fwrite($fp, "sep=;\n"); // Separator ; i nowa linia
            fputcsv($fp, ['Timestamp','Hosting','Type','Sorting','Primes','Fibonacci','Strings'], ';'); //Nagłówki kolumn
        }
        // Dodajemy znacznik czasu i hosting
        $timestamp = date("Y-m-d H:i:s");
        $hostingName = "infinityfree";

        //Wyniki każdego testu z osobna
        foreach ($allResults as $index => $row) 
        {
            $rowWithCommas = array_map(fn($v) => str_replace('.', ',', $v), $row);
            fputcsv($fp, array_merge(
                [$timestamp, $hostingName, "Test " . ($index + 1)],
                $rowWithCommas
            ), ';');
        }

        fclose($fp);
        echo "<p>Wyniki zapisane do pliku: wyniki_testu_algo.csv</p>";
    } else {
        echo "<p style='color:red;'>Błąd zapisu do pliku CSV!</p>";
    }
?>