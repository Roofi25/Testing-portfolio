const axios = require('axios');
const fs = require('fs');
const path = require('path');
const { performance } = require('perf_hooks');
const https = require('https');

// Konfiguracja
const iterations = 100;
const hostingName = "vercel";
const csvFile = path.join(__dirname, "wyniki_testu_vpn.csv");
const base = "https://vercel-testing-nu-indol.vercel.app/";

const urls = {
    "1KB": base + "response_1KB.js",
    "10KB": base + "response_10KB.js",
    "100KB": base + "response_100KB.js"
};

//Funkcja mierząca całkowity czas transmisji (od startu do pobrania danych)
async function measureRequest(url) {
    // cache buster
    const finalUrl = `${url}?cb=${Date.now()}_${Math.random().toString(36).substring(7)}`;
    const startTime = performance.now();
    try 
    {
        const agent = new https.Agent({ 
            keepAlive: false,
            rejectUnauthorized: false
        });

        const response = await axios.get(finalUrl, { 
            timeout: 15000,
            httpsAgent: agent,
            responseType: 'arraybuffer',
            headers: { 
                'Connection': 'close',
                'Cache-Control': 'no-cache, no-store, must-revalidate',
                'Pragma': 'no-cache' 
            }
        });

        const endTime = performance.now();

        return endTime - startTime; 
    } catch (error) {
        if (error.code === 'ECONNRESET' || error.message.includes('socket hang up')) {
            console.error(`[Hong Kong/VPN?] Połączenie zerwane (Socket Hang Up) dla: ${url}`);
        } else {
            console.error(`Błąd ${url}: ${error.message}`);
        }
        return 0; 
    }
}

async function runTests() {
    const allResults = [];

    for (let i = 1; i <= iterations; i++) {
        const row = {};
        row['1KB'] = await measureRequest(urls['1KB']);
        row['10KB'] = await measureRequest(urls['10KB']);
        row['100KB'] = await measureRequest(urls['100KB']);

        allResults.push(row);
    }

    saveToCsv(allResults);
}

// Funkcja zapisująca wyniki do CSV
function saveToCsv(results) {
    const fileExists = fs.existsSync(csvFile);
    let csvContent = "";

    if (!fileExists) {
        csvContent += "\ufeff"; // BOM dla Excela
        csvContent += "sep=;\n";
        csvContent += "Timestamp;Hosting;Test;1KB;10KB;100KB\n";
    }
    const date = new Date();
    const timestamp = date.toISOString().replace(/T/, ' ').replace(/\..+/, '');

    results.forEach((r, index) => {
        const val1 = r['1KB'].toFixed(2).replace('.', ',');
        const val10 = r['10KB'].toFixed(2).replace('.', ',');
        const val100 = r['100KB'].toFixed(2).replace('.', ',');

        csvContent += `${timestamp};${hostingName};Test ${index + 1};${val1};${val10};${val100}\n`;
    });

    // Pusty wiersz
    csvContent += ";;;;;\n";

    try {
        fs.appendFileSync(csvFile, csvContent, 'utf8');
        console.log(`\nWyniki zapisane do pliku: ${csvFile}`);
    } catch (err) {
        console.error("Błąd zapisu do pliku CSV! ", err);
    }
}

runTests();