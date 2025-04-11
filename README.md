SPECIFICHE WEB APPLICATION “RIDEWILD”
Web Application basata su MS .NET CORE 9.0, Back End, Framework Angular versione 19.x, Front End.
L’applicazione è di tipo SPA 🡪 Single Page Application
La sorgente dati è costituita dal Database SQL: AdventureWorksLT2019
N.B: Il DB in questione, può essere modificato SOLAMENTE in AGGIUNTA (tabelle, campi, viste, stored, etc.).
Obiettivi di Ridewild e funzionalità richieste
Gestione totale del DB come eCommerce, con prevalente compra vendita di MountainBike e pezzi di ricambio e articoli connessi (ad ogni modo, solo quello che è contemplato e contenuto delle tabelle products.
Ridewild è quindi una web app, per gli acquisti o anche solamente per curiosare tra i prodotti offerti.
Naturalmente, la curiosità deve essere gratuita: ossia, NO account, NO pre requisiti.
Se la curiosità, ossia la navigazione e la ricerca di prodotti, ha convinto il potenziale Cliente, se quest’ultimo vuole acquistare, deve registrarsi con un account sul Ridewild.
L’applicazione Ridewild, va mantenuta, va evoluta, va aggiornata, tutte attività che può espletare un utente con il ruolo di Amministratore.
Nelle attività dell’Amministrazione, rientrano anche gli aggiornamenti dei listini, prodotti, codice, quantità.
Si, possono esserci più amministratori: Admin, Magazzino, Ordini, etc.
Si, può esserci solo un utente Admin, e chi esegue il login con questo Utente, potrà fare tutto.
L’accesso tramite Account, implica una completa gestione del Login/Logout.
Anche se in forma minima, realizzare la gestione di un ordine/acquisto:
Procedura acquisti
Riempimento carrello
Conferma e elaborazione ordine
Elaborazione ordine, è sufficiente una conferma o via mail o come informazione storica del Cliente.
Aspetti tecnici di Ridewild
Gestione centralizzata errori (info a scelta), sia Front End, sia Back End.
Back End, patterns e architettura a scelta e/o in combinazione tra: EF, SqlClient, Multi DB, Db relazionali
Dati sensibili, credenziali, cifrate e memorizzate, dove decidete, NON in chiaro.
