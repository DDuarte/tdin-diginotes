# TDIN - Tecnologias de Distribuição e Integração
## .NET Remoting - Diginote Exchange System

ei11101 - **Duarte Duarte**  
ei11097 - **Ruben Cordeiro**

*2014/2015 - FEUP*

Compile
=======

1. Start Visual Studio 2013 or higher (.NET 4.5 required)
2. Open tdin-diginotes.sln
3. Compile

Execution
=========

1. Run Server.exe once
2. Run Client.exe multiple times (one per client)

Usage
=====

The application is fairly straightforward and simple to use, start
by registering an user by opening the tab REGISTER: Once logged in
it's possible to add funds ("developer" feature because everyone starts
with 0 € and 0 diginotes), see quotation history chart, create purchase
and sale orders, and other features (thoroughly described in the attached
report). Logging and persistence is done using the file 'transaction_log.txt'.

Dependencies
============

(All included with NuGet or .dlls)

- MvvmLight
- MahApps.Metro, MaterialDesign, Dragablz, MetroChart (UI)
- Newtonsoft.Json
