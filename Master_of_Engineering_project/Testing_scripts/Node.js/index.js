// Główne wymagania
const express = require('express');
const { DateTime } = require('luxon');
const path = require('path');
const { Client } = require('pg');

const app = express();

const PORT = process.env.PORT || 8080;

let lastDbResults = [];

// For security reasons, the true connection data were deleted
const connectionString = "postgres://<username>:<password>@<neon_host_url>/<database_name>";

// Strona główna z menu
app.get('/', (req, res) => {
  res.send(`
    <!DOCTYPE html>
    <html lang="pl">
    <head>
      <meta charset="UTF-8" />
      <title>Test Wydajności Hostingów</title>
      <style>
        body {
          font-family: Arial, sans-serif;
          background-color: #f8f8f8;
          padding: 20px;
          text-align: center;
        }
        a {
          color: #007bff;
          font-weight: bold;
          text-decoration: none;
        }
      </style>
    </head>
    <body>
      <h1>Witamy na stronie testowej</h1>
      <p><a href="/db_test">Kliknij tutaj, aby uruchomić test bazy danych</a></p>
      <p><a href="/algo_test">Kliknij tutaj, aby uruchomić test algorytmiczny</a></p>
      <p><a href="/knn_test">Kliknij tutaj, aby uruchomić test KNN</a></p>
    </body>
    </html>
  `);
});

app.get('/db_test', async (req, res) => {

  const client = new Client({ connectionString });

  try {
    await client.connect();

    await client.query("DROP TABLE IF EXISTS performance_test");

    await client.query(`
      CREATE TABLE performance_test (
        id SERIAL PRIMARY KEY,
        text_val TEXT NOT NULL,
        int_val INT NOT NULL,
        float_val REAL NOT NULL,
        dec_val NUMERIC(10,2) NOT NULL,
        date_val DATE NOT NULL
      )
    `);

    const iteracje = 5;
    const allResults = [];

    for (let t = 0; t < iteracje; t++) {
      const wyniki = {};

      const insertValues = [];
      const insertParams = [];

      const insertStart = process.hrtime.bigint();

      for (let i = 0; i < 5000; i++) {
        insertParams.push(
          'test'.repeat(50),
          Math.floor(Math.random()*100000),
          Math.random()*1000,
          Math.floor(Math.random()*1000000)/100,
          new Date(Date.now() + i*86400000)
        );

        insertValues.push(
          `($${insertParams.length-4},$${insertParams.length-3},$${insertParams.length-2},$${insertParams.length-1},$${insertParams.length})`
        );
      }

      await client.query(
        `INSERT INTO performance_test (text_val,int_val,float_val,dec_val,date_val)
         VALUES ${insertValues.join(',')}`,
        insertParams
      );

      wyniki.INSERT = Number(process.hrtime.bigint() - insertStart)/1_000_000;

      const selectStart = process.hrtime.bigint();
      await client.query("SELECT * FROM performance_test");
      wyniki.SELECT = Number(process.hrtime.bigint() - selectStart)/1_000_000;

      const updateRows = [];
      const updateParams = [];
      const updateStart = process.hrtime.bigint();

      for (let i = 1; i <= 5000; i++) {
        updateParams.push(
          i,
          'upd'.repeat(50),
          Math.floor(Math.random()*100000),
          Math.random()*1000,
          Math.floor(Math.random()*1000000)/100,
          new Date(Date.now() - i*86400000)
        );

        const base = updateParams.length;
        updateRows.push(
          `($${base-5}::INTEGER, $${base-4}::TEXT, $${base-3}::INTEGER, $${base-2}::REAL, $${base-1}::NUMERIC, $${base}::DATE)`
        );
      }

      await client.query(
        `
        UPDATE performance_test AS p SET
          text_val = u.text_val,
          int_val  = u.int_val,
          float_val = u.float_val,
          dec_val = u.dec_val,
          date_val = u.date_val
        FROM (VALUES ${updateRows.join(',')})
        AS u(id,text_val,int_val,float_val,dec_val,date_val)
        WHERE p.id = u.id::INTEGER;
        `,
        updateParams
      );

      wyniki.UPDATE = Number(process.hrtime.bigint() - updateStart)/1_000_000;

      const deleteStart = process.hrtime.bigint();
      await client.query("DELETE FROM performance_test");
      wyniki.DELETE = Number(process.hrtime.bigint() - deleteStart)/1_000_000;

      allResults.push(wyniki);
    }

    await client.end();

    lastDbResults = allResults;

    let html = `<h2>Wyniki testu bazy danych (${iteracje} iteracji)</h2>
    <table border="1" cellpadding="8" cellspacing="0">
      <tr>
        <th style="text-align:center">Test</th>
        <th style="text-align:center">INSERT</th>
        <th style="text-align:center">SELECT</th>
        <th style="text-align:center">UPDATE</th>
        <th style="text-align:center">DELETE</th>
      </tr>`;

    allResults.forEach((r, i) => {
      html += `<tr>
        <td style="text-align:center">Test ${i + 1}</td>
        <td style="text-align:center">${r.INSERT.toFixed(2)} ms</td>
        <td style="text-align:center">${r.SELECT.toFixed(2)} ms</td>
        <td style="text-align:center">${r.UPDATE.toFixed(2)} ms</td>
        <td style="text-align:center">${r.DELETE.toFixed(2)} ms</td>
      </tr>`;
    });

    const summary = {};
    ['INSERT','SELECT','UPDATE','DELETE'].forEach(op => {
      const times = allResults.map(r => r[op]);
      const count = times.length;
      const avg = times.reduce((a,b)=>a+b,0)/count;
      const min = Math.min(...times);
      const max = Math.max(...times);
      const median = count % 2 === 0 
        ? (times[count/2 - 1] + times[count/2])/2 
        : times[Math.floor(count/2)];
      const variance = times.reduce((v,x)=>v + (x-avg)**2,0)/count;
      const stdev = Math.sqrt(variance);
      const stability = max/min;
      summary[op] = {avg,min,max,median,stdev,stability};
    });

    for (const [key,label] of Object.entries({avg:'AVG',min:'MIN',max:'MAX',median:'MEDIAN',stdev:'STDEV',stability:'STABILITY'})) {
      html += `<tr><th style="text-align:center">${label}</th>`;
      ['INSERT','SELECT','UPDATE','DELETE'].forEach(op => {
        html += `<th style="text-align:center">${
          key === 'stability' ? summary[op][key].toFixed(2) : summary[op][key].toFixed(2)+' ms'
        }</th>`;
      });
      html += `</tr>`;
    }
    html += `</table>`;
    html += `<p><a href="/download_db_csv" download>Pobierz plik csv z wynikami</a></p>`;
    res.send(`<html><head><meta charset="UTF-8"></head><body>${html}</body></html>`);

  } catch (err) {
    res.send(`<p style="color:red;">Błąd: ${err.message}</p>`);
  }
});

app.get('/download_db_csv', (req, res) => {
  if (!lastDbResults || lastDbResults.length === 0) {
    return res.status(400).send('Brak danych — najpierw uruchom /db_test');
  }

  let csv = '\uFEFF';
  csv += 'sep=;\n';
  csv += 'Timestamp;Hosting;Test;INSERT;SELECT;UPDATE;DELETE\n';

  const timestamp = formatDate(new Date());
  const hosting = "vercel";

  lastDbResults.forEach((r,i) => {
    csv += [
      timestamp,
      hosting,
      `Test ${i+1}`,
      formatNumber(r.INSERT),
      formatNumber(r.SELECT),
      formatNumber(r.UPDATE),
      formatNumber(r.DELETE)
    ].join(';') + '\n';
  });

  res.setHeader('Content-Type', 'text/csv; charset=UTF-8');
  res.setHeader('Content-Disposition', 'attachment; filename="wyniki_testu_db.csv"');
  res.send(csv);
});

// Funkcje testów (1 iteracja)
function testSorting(size = 2_000_000) {
  const arr = Array.from({ length: size }, () => Math.floor(Math.random() * 1_000_000));
  const start = process.hrtime.bigint();
  arr.sort((a, b) => a - b);
  const end = process.hrtime.bigint();
  return Number(end - start) / 1e6; // ms
}

function testPrimes(limit = 2_000_000) {
  const start = process.hrtime.bigint();
  const sieve = Array(limit + 1).fill(true);
  sieve[0] = sieve[1] = false;
  for (let i = 2; i * i <= limit; i++) {
    if (sieve[i]) {
      for (let j = i * i; j <= limit; j += i) sieve[j] = false;
    }
  }
  const end = process.hrtime.bigint();
  return Number(end - start) / 1e6;
}

function testFibonacci(n = 30_000_000) {
  const start = process.hrtime.bigint();
  let a = 0, b = 1;
  for (let i = 2; i <= n; i++) [a, b] = [b, a + b];
  const end = process.hrtime.bigint();
  return Number(end - start) / 1e6;
}

function testStringOps(iterations = 30_000_000) {
  const start = process.hrtime.bigint();
  let str = 'a';
  for (let i = 0; i < iterations; i++) {
    str += 'b';
    if (str.length > 1000) str = str.slice(500);
  }
  const end = process.hrtime.bigint();
  return Number(end - start) / 1e6;
}

// Funkcja pomocnicza do konwersji liczb na format z przecinkiem
function formatNumber(num) {
  return num.toFixed(2).replace('.', ',');
}

// Funkcja do formatowania daty w stylu dd.mm.yyyy hh:mm
function formatDate(date) {
  return DateTime.fromJSDate(date)
    .setZone('Europe/Warsaw')      // ustawienie strefy czasowej
    .toFormat('dd.MM.yyyy HH:mm'); // formatowanie
}

//zmienna globalna przechowująca wyniki testu
let lastAlgoResults = [];

// Strona testu algorytmicznego
app.get('/algo_test', (req, res) => {
  const iterations = 5;
  const algorithms = ['SORTING','PRIMES','FIBONACCI','STRINGS'];
  const allResults = [];

  for (let t = 0; t < iterations; t++) {
    const results = {
      timestamp: formatDate(new Date()),
      hosting: 'vercel',
      SORTING: testSorting(),
      PRIMES: testPrimes(),
      FIBONACCI: testFibonacci(),
      STRINGS: testStringOps()
    };
    allResults.push(results);
  }

  lastAlgoResults = allResults;

  // Liczenie statystyk
  const summary = {};
  for (const alg of algorithms) {
    const times = allResults.map(r => r[alg]).sort((a,b) => a-b);
    const count = times.length;
    const avg = times.reduce((a,b)=>a+b,0)/count;
    const min = times[0];
    const max = times[count-1];
    const median = (count % 2 === 0) ? (times[count/2 -1]+times[count/2])/2 : times[Math.floor(count/2)];
    const variance = times.reduce((v,x)=>v + (x-avg)**2,0)/count;
    const stdev = Math.sqrt(variance);
    const stability = max/min;
    summary[alg] = {avg,min,max,median,stdev,stability};
  }

  // Plik CSV w pamięci, który pobieramy ręcznie ze strony jako że 
  // Vercel jest rozwiązaniem serverless
  let csv = '\uFEFF'; // UTF-8 BOM
  csv += 'sep=;\n';
  csv += 'Test;Sorting;Primes;Fibonacci;Strings\n';
  allResults.forEach((res,i) => {
    csv += [
      "Test " + (i+1),
      formatNumber(res.SORTING),
      formatNumber(res.PRIMES),
      formatNumber(res.FIBONACCI),
      formatNumber(res.STRINGS)
    ].join(';') + '\n';
  });

  // Tworzenie tabeli HTML
  let tableHTML = `
  <h2>Wyniki testu algorytmicznego (${iterations} iteracji)</h2>
  <table border="1" cellpadding="8" cellspacing="0">
    <tr>
      <th style="text-align:center">Test</th>
      <th style="text-align:center">Sortowanie</th>
      <th style="text-align:center">Liczby pierwsze</th>
      <th style="text-align:center">Fibonacci</th>
      <th style="text-align:center">Operacje na stringach</th>
    </tr>`;
  allResults.forEach((res,i) => {
    tableHTML += `<tr><td style="text-align:center">Test ${i+1}</td>`;
    for (const alg of algorithms) tableHTML += `<td style="text-align:center">${res[alg].toFixed(2)} ms</td>`;
    tableHTML += `</tr>`;
  });

  // Dodanie statystyk
  for (const [key,label] of Object.entries({avg:'AVG',min:'MIN',max:'MAX',median:'MEDIAN',stdev:'STDEV',stability:'STABILITY'})) {
    tableHTML += `<tr><th style="text-align:center">${label}</th>`;
    for (const alg of algorithms) {
      const val = summary[alg][key];
      tableHTML += `<th style="text-align:center">${key==='stability'?val.toFixed(2):val.toFixed(2)+' ms'}</th>`;
    }
    tableHTML += `</tr>`;
  }

  tableHTML += `</table>`;
  //Zamieszamy link do pobrania pliku w htmlu
  tableHTML += `<p><a href="/download_algo_csv" download>Pobierz plik csv z wynikami</a></p>`;
  res.send(`<html><head><meta charset="UTF-8"></head><body>${tableHTML}</body></html>`);
});

// Endpoint do pobrania pliku csv
// Będzie on tworzony bezpośrednio w pamięci ze względu, że 
// Vercel jest rozwiązaniem serverless.
app.get('/download_algo_csv', (req, res) => {
  if (!lastAlgoResults || lastAlgoResults.length === 0) {
    return res.status(400).send('Nie ma danych do pobrania. Najpierw przeprowadź test!');
  }

  const iterations = lastAlgoResults.length;
  const algorithms = ['SORTING','PRIMES','FIBONACCI','STRINGS'];
  
  let csv = '\uFEFF';
  csv += 'sep=;\n';
  csv += 'Timestamp;Hosting;Type;Sorting;Primes;Fibonacci;Strings\n';
  lastAlgoResults.forEach((res,i) => {
    csv += [
      res.timestamp,
      res.hosting,
      "Test " + (i+1),
      formatNumber(res.SORTING),
      formatNumber(res.PRIMES),
      formatNumber(res.FIBONACCI),
      formatNumber(res.STRINGS)
    ].join(';') + '\n';
  });

  res.setHeader('Content-Type', 'text/csv; charset=UTF-8');
  res.setHeader('Content-Disposition', 'attachment; filename="wyniki_testu_algo.csv"');
  res.send(csv);
});

//KNN
let lastKnnResults = []; // Zmienna do przechowywania wyników w pamięci

// Funkcja k-NN (klasyfikacja 500 punktów na podstawie 200 treningowych)
function testKNN(pointsTrain = 200, pointsTest = 500, k = 5) {
    // Stałe ziarno dla powtarzalności (odpowiednik mt_srand w PHP)
    // Dzięki temu na każdym hostingu dane będą identyczne
    let seed = 12345;
    function pseudorandom() {
        seed = (seed * 16807) % 2147483647;
        return (seed - 1) / 2147483646;
    }

    const trainingSet = [];
    for (let i = 0; i < pointsTrain; i++) {
        trainingSet.push({
            x: Math.floor(pseudorandom() * 1001),
            y: Math.floor(pseudorandom() * 1001),
            label: Math.floor(pseudorandom() * 2)
        });
    }

    const testingSet = [];
    for (let i = 0; i < pointsTest; i++) {
        testingSet.push({
            x: Math.floor(pseudorandom() * 1001),
            y: Math.floor(pseudorandom() * 1001)
        });
    }

    const start = process.hrtime.bigint();

    // Główna pętla klasyfikacji
    for (const p of testingSet) {
        const distances = [];
        for (const t of trainingSet) {
            // Odległość euklidesowa (bez pierwiastka dla wydajności)
            const dist = Math.pow(p.x - t.x, 2) + Math.pow(p.y - t.y, 2);
            distances.push({ d: dist, l: t.label });
        }

        // Sortowanie odległości
        distances.sort((a, b) => a.d - b.d);

        // Głosowanie k-sąsiadów
        const votes = { 0: 0, 1: 0 };
        for (let i = 0; i < k; i++) {
            votes[distances[i].l]++;
        }
        // Wynik klasyfikacji (niezapisywany, bo mierzymy czas operacji)
        const result = votes[0] > votes[1] ? 0 : 1;
    }

    const end = process.hrtime.bigint();
    // Konwersja nanosekund na milisekund (1ms = 1 000 000 ns)
    return Number(end - start) / 1000000;
}

// Endpoint KNN - wykonuje test i zapisuje wyniki w pamięci
app.get('/knn_test', (req, res) => {
    const iterations = 100;
    const results = [];

    for (let i = 0; i < iterations; i++) {
        results.push(testKNN());
    }

    // Zapisujemy wyniki do zmiennej globalnej zamiast do pliku
    lastKnnResults = results;

    // Wysyłamy odpowiedź z linkiem do pobrania
    res.send(`
        <p><a href="/download_knn_csv" download>Pobierz plik csv z wynikami</a></p>
    `);
});

// Nowy endpoint do pobierania pliku CSV z pamięci
app.get('/download_knn_csv', (req, res) => {
    if (!lastKnnResults || lastKnnResults.length === 0) {
        return res.status(400).send('Brak danych KNN — najpierw uruchom /knn_test');
    }

    let csv = '';
    lastKnnResults.forEach((time) => {
        // Formatowanie pod polski Excel (przecinek zamiast kropki)
        const formattedTime = time.toFixed(2).replace('.', ',');
        csv += formattedTime + '\n';
    });

    // Ustawiamy nagłówki, żeby przeglądarka wiedziała, że to plik do pobrania
    res.setHeader('Content-Type', 'text/csv; charset=UTF-8');
    res.setHeader('Content-Disposition', 'attachment; filename="wyniki_testu_knn.csv"');
    
    // Wysyłamy gotowy tekst CSV
    res.send(csv);
});



const data1KB = "A".repeat(1024);
const data10KB = "A".repeat(10240);
const data100KB = "A".repeat(102400);

// Endpointy dla testu vpn
app.get('/response_1KB.js', (req, res) => {
    res.setHeader('Content-Type', 'text/plain');
    res.send(data1KB);
});

app.get('/response_10KB.js', (req, res) => {
    res.setHeader('Content-Type', 'text/plain');
    res.send(data10KB);
});

app.get('/response_100KB.js', (req, res) => {
    res.setHeader('Content-Type', 'text/plain');
    res.send(data100KB);
});

// Ednpointy dla testów w jmeter
// Strona statyczna
app.get('/static', (req, res) => {
    res.setHeader('Content-Type', 'text/plain');
    res.send("OK");
});

// JSON - pobranie ok. 3KB danych strukturalnych
app.get('/json', (req, res) => {
    res.setHeader('Content-Type', 'application/json');
    const data = Array.from({ length: 50 }, (v, i) => ({
        id: i,
        status: "active",
        label: "test_data_point",
        timestamp: Date.now()
    }));
    res.json(data);
});

// Wyliczanie w pętli sumy pierwiastków
app.get('/compute', (req, res) => {
    let sum = 0;
    for (let i = 0; i < 100000; i++) {
        sum += Math.sqrt(i);
    }
    res.send(`Result: ${sum.toFixed(2)}`);
});

app.listen(PORT, '0.0.0.0', () => console.log(`Server running on port ${PORT}`));
