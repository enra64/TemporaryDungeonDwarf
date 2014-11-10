TemporaryDungeonDwarf
=====================

muss nicht dungeon dwarf heißen. ist das repo für unser spiel. bitte erst klonen, und dann euern kram in das vorhandene projekt einfügen, um pushkonflikten vorzubeugen

Aktuelle Aufagebenverteilung:
Arne: Welt/Seiten evtl mit Tilemaps und Views
Hans-Martin: Gegner:
Healthbar, Verschiedene Waffenklassen benutzen
Daniel: Spieler:
Healthbar, waffen benutzen, position/größe/richtung offenlegen!
Edgar: Fernkampf implementieren:
auf Daniels Spieler zugreifen (kriegst du im constructor), erstmal eine kugel schreiben,
welche sich bei aufrufen zu Update() weiterbewegt und zu Draw() selber auf die map malen kann;
je nachdem ob du willst oder nicht evtl auch ne kollisionsabfrage mit objekten der welt;
danach auch noch ne collisiontesting gegen ein objekt, dessen größe und position du übergeben bekommst
(e.g. public bool TestCollision(Vector2f size, Vector2f position))
Pascal:Schwert implementieren
Du kriegst auch im constructor ne referenz zu daniels spieler, und kannst dir für jeden abruf von z.B. 
HitForward() oder Update die position von Daniel holen. Collisionstesting ist bei dir glaube ich am 
besten ähnlich wie bei edgar, dass du ne funktion schreibst die zurückgibt ob das schwert momentan damit kollidiert
