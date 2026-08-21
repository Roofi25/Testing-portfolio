<?php
// Infinityfree hosting – test czasu odpowiedzi HTTP (Całkowity Czas Transmisji dla 1KB, 10KB, 100KB)

    date_default_timezone_set('Europe/Warsaw');
    $csvFile = __DIR__ . "/wyniki_testu_vpn.csv";
    
    $base = "https://testingthewebsite.infinityfreeapp.com/"; 

    $urls = [
        "1KB" 	=> $base . "response_1KB.php",
        "10KB" 	=> $base . "response_10KB.php",
        "100KB" => $base . "response_100KB.php"
    ];

    // Funkcja która mierzy całkowity czas transmisji (total_time)
    function measureRequest($url) {
        if (!function_exists('curl_init')) {
            throw new Exception("Rozszerzenie CURL nie jest zainstalowane w Twoim PHP!");
        }

        $ch = curl_init($url);
        
        // Standardowe ustawienia CURL
        // Zwraca odpowiedź jako ciąg znaków zamiast ją bezpośrednio wyświetlać
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        // Automatyczne podążanie za przekierowaniami HTTP (np. z HTTP na HTTPS)
        curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
        // Maksymalny czas oczekiwania na całą transakcję (30 sekund)
        curl_setopt($ch, CURLOPT_TIMEOUT, 30);
        // Wyłączenie weryfikacji certyfikatu SSL
        curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false); 
        
        // Wykonanie żądania HTTP
        curl_exec($ch);
        
        // Pobranie szczegółowych informacji o czasach transakcji
        $info = curl_getinfo($ch);
        curl_close($ch);

        // Pobranie całkowitego czasu transakcji (od startu do końca) w sekundach
        $time = $info['total_time'];
        
        // Konwersja czasu z sekund na milisekundy (ms) i zaokrąglenie do 2 miejsc po przecinku
        return round($time * 1000, 2); 
    }

    // Liczba iteracji testu
    $iterations = 100;
    $allResults = [];

    for ($i = 1; $i <= $iterations; $i++) {
        $row = [];
        try {
            // Mierzymy całkowity czas dla 1KB
            $row['1KB'] = measureRequest($urls['1KB']);
            
            // Mierzymy całkowity czas dla 10KB
            $row['10KB'] = measureRequest($urls['10KB']);
            
            // Mierzymy całkowity czas dla 100KB
            $row['100KB'] = measureRequest($urls['100KB']);
            
            $allResults[] = $row;
        } catch (Exception $e) {
            echo "<p style='color:red;'>Błąd w teście {$i}: " . $e->getMessage() . "</p>";
            $allResults[] = ['1KB' => '0', '10KB' => '0', '100KB' => '0']; 
        }
    }

    /*
    // === WYŚWIETLENIE WYNIKÓW W HTML ===
    echo "<h2>Wyniki pomiaru latencji i przepustowości HTTP dla różnych lokalizacji (".$iterations." iteracji)</h2>";
    echo "<table border='1' cellpadding='8' cellspacing='0'>
    <tr>
        <th style='text-align:center'>Test</th>
        <th style='text-align:center'>1 KB (TTFB)</th>
        <th style='text-align:center'>10 KB (Całkowity)</th>
        <th style='text-align:center'>100 KB (Całkowity)</th>
    </tr>";

    foreach ($allResults as $index => $r) {
        $t = $index + 1;
        echo "<tr>
            <td style='text-align:center'>Test $t</td>
            <td style='text-align:center'>{$r['1KB']} ms</td>
            <td style='text-align:center'>{$r['10KB']} ms</td>
            <td style='text-align:center'>{$r['100KB']} ms</td>
        </tr>";
    }

    echo "</table>";
    */

    // === ZAPISANIE WYNIKÓW DO PLIKU CSV ===
    $newFile = !file_exists($csvFile);
    $fp = fopen($csvFile, "a");

    if ($fp) {
        if ($newFile) {
            fwrite($fp, "\xEF\xBB\xBF");
            fwrite($fp, "sep=;\n");
            fputcsv($fp, ["Timestamp","Hosting","Test","1KB","10KB","100KB"], ";");
        }

        $timestamp = date("Y-m-d H:i:s");
        $hostingName = "infinityfree"; 

        foreach ($allResults as $index => $r) {
            $values = array_map(fn($v) => str_replace('.', ',', $v), $r);

            fputcsv($fp, [
                $timestamp, 
                $hostingName,
                "Test " . ($index + 1),
                $values["1KB"],
                $values["10KB"],
                $values["100KB"]
            ], ";");
        }

        // Dodanie pustego wiersza dla oddzielenia lokalizacji
        fputcsv($fp, ["", "", "", "", "", ""], ";"); 

        fclose($fp);
        echo "<p>Wyniki zapisane do pliku: {$csvFile}</p>";
    } else {
        echo "<p style='color:red;'>Błąd zapisu do pliku CSV!</p>";
    }
?>