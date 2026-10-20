using System.Collections.Generic;

namespace Watermelon
{
    /// <summary>
    /// Every translated text. One row per text, columns in <see cref="GameLanguage"/> order:
    /// English, Hinglish, Spanish, Portuguese, Indonesian, French, German, Turkish.
    /// Leave a column empty ("") to show the English text. Add a row to translate a new text;
    /// the key is an id ("tut.tray") or the English text itself.
    /// </summary>
    public static partial class LocTable
    {
        public static readonly Dictionary<string, string[]> Texts = new Dictionary<string, string[]>();

        private static void Add(string key, params string[] row) => Texts[key] = row;

        static LocTable()
        {
            Tutorial();
            Common();
            Panels();
            Menus();
            ArtWords();
        }

        // ------------------------------------------------------------------ tutorial
        private static void Tutorial()
        {
            Add("tut.tray",
                "Look at the tray's colour! Tap a dish of that same colour below.",
                "Tray ka colour dekho! Neeche wahi colour ki dish par tap karo.",
                "¡Mira el color de la bandeja! Toca abajo un plato del mismo color.",
                "Olhe a cor da bandeja! Toque abaixo num prato da mesma cor.",
                "Lihat warna nampannya! Ketuk hidangan dengan warna yang sama di bawah.",
                "Regarde la couleur du plateau ! Touche en bas un plat de la même couleur.",
                "Sieh dir die Farbe des Tabletts an! Tippe unten auf ein Gericht in derselben Farbe.",
                "Tepsinin rengine bak! Aşağıda aynı renkteki bir yemeğe dokun.");
            Add("tut.seat1",
                "Great! A tray takes 3 dishes - 2 more!",
                "Shabaash! Ek tray mein 3 dishes jaati hain - 2 aur!",
                "¡Genial! En una bandeja caben 3 platos: ¡faltan 2!",
                "Ótimo! Uma bandeja leva 3 pratos - faltam 2!",
                "Hebat! Satu nampan muat 3 hidangan - 2 lagi!",
                "Bravo ! Un plateau contient 3 plats - encore 2 !",
                "Super! Auf ein Tablett passen 3 Gerichte - noch 2!",
                "Harika! Bir tepsiye 3 yemek sığar - 2 tane daha!");
            Add("tut.seat2",
                "Just one more!",
                "Bas ek aur!",
                "¡Solo uno más!",
                "Só mais um!",
                "Satu lagi!",
                "Plus qu'un !",
                "Nur noch eins!",
                "Sadece bir tane daha!");
            Add("tut.full",
                "Great! The tray is full and the customer left happy. Every new tray needs 3 dishes of its colour.",
                "Shabaash! Tray bhar gayi aur customer khush hokar chala gaya. Har nayi tray ko uske colour ki 3 dishes chahiye.",
                "¡Genial! La bandeja está llena y el cliente se fue feliz. Cada bandeja nueva necesita 3 platos de su color.",
                "Ótimo! A bandeja encheu e o cliente saiu feliz. Cada bandeja nova precisa de 3 pratos da sua cor.",
                "Hebat! Nampan penuh dan pelanggan pergi dengan senang. Setiap nampan baru butuh 3 hidangan sewarna.",
                "Bravo ! Le plateau est plein et le client repart content. Chaque nouveau plateau veut 3 plats de sa couleur.",
                "Super! Das Tablett ist voll und der Gast geht glücklich. Jedes neue Tablett braucht 3 Gerichte in seiner Farbe.",
                "Harika! Tepsi doldu ve müşteri mutlu ayrıldı. Her yeni tepsiye kendi renginde 3 yemek gerekir.");
            Add("tut.dock",
                "A dish of the wrong colour waits down here. There are only 7 spots - if they all fill up, you lose the level!",
                "Galat colour ki dish yahan neeche ruk jaati hai. Sirf 7 jagah hain - sab bhar gayi to level haar jaoge!",
                "Un plato del color equivocado espera aquí abajo. Solo hay 7 huecos: ¡si se llenan todos, pierdes el nivel!",
                "Um prato da cor errada espera aqui embaixo. Só há 7 lugares - se todos encherem, você perde a fase!",
                "Hidangan dengan warna yang salah menunggu di bawah sini. Hanya ada 7 tempat - kalau penuh semua, kamu kalah!",
                "Un plat de la mauvaise couleur attend ici en bas. Il n'y a que 7 places - si elles sont toutes prises, tu perds le niveau !",
                "Ein Gericht in der falschen Farbe wartet hier unten. Es gibt nur 7 Plätze - sind alle voll, verlierst du das Level!",
                "Yanlış renkteki yemek burada aşağıda bekler. Sadece 7 yer var - hepsi dolarsa bölümü kaybedersin!");
            Add("tut.orders",
                "Check ORDERS at the top to see what's needed. Complete all the orders to win the level!",
                "Upar ORDERS mein dekho kya chahiye. Saare orders poore karo aur level jeeto!",
                "Mira los PEDIDOS arriba para ver qué hace falta. ¡Completa todos los pedidos y gana el nivel!",
                "Veja os PEDIDOS lá em cima para saber o que falta. Complete todos os pedidos e vença a fase!",
                "Lihat PESANAN di atas untuk tahu yang dibutuhkan. Selesaikan semua pesanan dan menangkan level!",
                "Regarde les COMMANDES en haut pour voir ce qu'il faut. Termine toutes les commandes pour gagner le niveau !",
                "Oben bei BESTELLUNGEN siehst du, was gebraucht wird. Erfülle alle Bestellungen und gewinne das Level!",
                "Neyin gerektiğini görmek için üstteki SİPARİŞLER'e bak. Tüm siparişleri tamamla ve bölümü kazan!");
            Add("tut.done",
                "That's it! Now try it yourself.",
                "Bas itna hi! Ab tum khud try karo.",
                "¡Eso es todo! Ahora pruébalo tú.",
                "É isso! Agora tente você.",
                "Itu saja! Sekarang coba sendiri.",
                "C'est tout ! À toi de jouer.",
                "Das war's! Jetzt bist du dran.",
                "Hepsi bu kadar! Şimdi sen dene.");
            Add("tut.hidden",
                "This dish is covered! Clear the dishes around it to reveal it.",
                "Yeh dish dhakki hui hai! Iske aas-paas ki dishes hatao, tab yeh khulegi.",
                "¡Este plato está tapado! Quita los platos de alrededor para descubrirlo.",
                "Este prato está coberto! Tire os pratos ao redor para revelá-lo.",
                "Hidangan ini tertutup! Singkirkan hidangan di sekitarnya agar terbuka.",
                "Ce plat est couvert ! Enlève les plats autour pour le découvrir.",
                "Dieses Gericht ist abgedeckt! Räum die Gerichte drumherum weg, dann wird es sichtbar.",
                "Bu yemek kapalı! Açılması için etrafındaki yemekleri kaldır.");
            Add("tut.spawner",
                "This green box gives new dishes! The number shows how many are left inside. Take the dish in front of it and the next one comes out by itself.",
                "Yeh hara dabba nayi dishes deta hai! Number batata hai iske andar kitni dishes baaki hain. Iske saamne wali dish uthao, agli apne aap bahar aayegi.",
                "¡Esta caja verde da platos nuevos! El número indica cuántos quedan dentro. Toma el plato de delante y el siguiente saldrá solo.",
                "Esta caixa verde dá pratos novos! O número mostra quantos restam lá dentro. Pegue o prato da frente e o próximo sai sozinho.",
                "Kotak hijau ini memberi hidangan baru! Angkanya menunjukkan sisa hidangan di dalam. Ambil hidangan di depannya, yang berikutnya keluar sendiri.",
                "Cette boîte verte donne de nouveaux plats ! Le nombre indique combien il en reste. Prends le plat devant elle et le suivant sort tout seul.",
                "Diese grüne Kiste gibt neue Gerichte! Die Zahl zeigt, wie viele noch drin sind. Nimm das Gericht davor und das nächste kommt von selbst heraus.",
                "Bu yeşil kutu yeni yemekler verir! Sayı içeride kaç tane kaldığını gösterir. Önündeki yemeği al, sonraki kendiliğinden çıkar.");
            Add("tut.undo",
                "UNDO: made a mistake? The last dish goes back to the board.",
                "UNDO: galti ho gayi? Aakhri dish wapas board par chali jaayegi.",
                "DESHACER: ¿te equivocaste? El último plato vuelve al tablero.",
                "DESFAZER: errou? O último prato volta para o tabuleiro.",
                "URUNGKAN: salah langkah? Hidangan terakhir kembali ke papan.",
                "ANNULER : une erreur ? Le dernier plat retourne sur le plateau.",
                "RÜCKGÄNGIG: vertan? Das letzte Gericht kommt zurück aufs Brett.",
                "GERİ AL: hata mı yaptın? Son yemek tahtaya geri döner.");
            Add("tut.hint",
                "HINT: stuck? It sends the right dishes to the tray for you.",
                "HINT: phas gaye? Yeh sahi dishes khud tray tak bhej deta hai.",
                "PISTA: ¿atascado? Envía los platos correctos a la bandeja por ti.",
                "DICA: travou? Ela manda os pratos certos para a bandeja por você.",
                "PETUNJUK: buntu? Ini mengirim hidangan yang tepat ke nampan untukmu.",
                "INDICE : bloqué ? Il envoie les bons plats sur le plateau pour toi.",
                "TIPP: festgefahren? Er schickt die richtigen Gerichte für dich aufs Tablett.",
                "İPUCU: takıldın mı? Doğru yemekleri senin için tepsiye gönderir.");
            Add("tut.shuffle",
                "SHUFFLE: mixes the dishes on the board and opens new paths.",
                "SHUFFLE: board ki dishes mix ho jaati hain, naye raste khul jaate hain.",
                "MEZCLAR: revuelve los platos del tablero y abre caminos nuevos.",
                "EMBARALHAR: mistura os pratos do tabuleiro e abre novos caminhos.",
                "ACAK: mengacak hidangan di papan dan membuka jalan baru.",
                "MÉLANGER : mélange les plats du plateau et ouvre de nouveaux chemins.",
                "MISCHEN: mischt die Gerichte auf dem Brett und öffnet neue Wege.",
                "KARIŞTIR: tahtadaki yemekleri karıştırır ve yeni yollar açar.");
            Add("tut.next", "NEXT", "AAGE", "SIGUIENTE", "PRÓXIMO", "LANJUT", "SUIVANT", "WEITER", "İLERİ");
            Add("tut.gotit", "GOT IT!", "SAMAJH GAYA!", "¡ENTENDIDO!", "ENTENDI!", "MENGERTI!", "COMPRIS !", "VERSTANDEN!", "ANLADIM!");
            Add("tut.play", "LET'S PLAY!", "CHALO KHELEIN!", "¡A JUGAR!", "VAMOS JOGAR!", "AYO MAIN!", "ON JOUE !", "LOS GEHT'S!", "HADİ OYNA!");
            Add("tut.skip", "SKIP TUTORIAL >>", "TUTORIAL SKIP >>", "SALTAR TUTORIAL >>", "PULAR TUTORIAL >>", "LEWATI TUTORIAL >>", "PASSER LE TUTORIEL >>", "TUTORIAL ÜBERSPRINGEN >>", "EĞİTİMİ ATLA >>");
        }

        // ------------------------------------------------------------------ settings & shared
        private static void Common()
        {
            Add("LANGUAGE", "LANGUAGE", "LANGUAGE", "IDIOMA", "IDIOMA", "BAHASA", "LANGUE", "SPRACHE", "DİL");
            Add("VERSION", "VERSION", "VERSION", "VERSIÓN", "VERSÃO", "VERSI", "VERSION", "VERSION", "SÜRÜM");
            Add("SETTINGS", "SETTINGS", "SETTINGS", "AJUSTES", "CONFIGURAÇÕES", "PENGATURAN", "PARAMÈTRES", "EINSTELLUNGEN", "AYARLAR");
            Add("SOUND", "SOUND", "SOUND", "SONIDO", "SOM", "SUARA", "SON", "TON", "SES");
            Add("VIBRATION", "VIBRATION", "VIBRATION", "VIBRACIÓN", "VIBRAÇÃO", "GETAR", "VIBRATION", "VIBRATION", "TİTREŞİM");
            Add("ON", "ON", "ON", "SÍ", "LIGADO", "AKTIF", "OUI", "AN", "AÇIK");
            Add("OFF", "OFF", "OFF", "NO", "DESLIGADO", "MATI", "NON", "AUS", "KAPALI");
            Add("MENU", "MENU", "MENU", "MENÚ", "MENU", "MENU", "MENU", "MENÜ", "MENÜ");
            Add("CLOSE", "CLOSE", "CLOSE", "CERRAR", "FECHAR", "TUTUP", "FERMER", "SCHLIESSEN", "KAPAT");
        }

        // ------------------------------------------------------------------ panel titles
        private static void Panels()
        {
            Add("SHOP", "SHOP", "SHOP", "TIENDA", "LOJA", "TOKO", "BOUTIQUE", "SHOP", "MAĞAZA");
            Add("COLLECTION", "COLLECTION", "COLLECTION", "COLECCIÓN", "COLEÇÃO", "KOLEKSI", "COLLECTION", "SAMMLUNG", "KOLEKSİYON");
            Add("ACHIEVEMENTS", "ACHIEVEMENTS", "ACHIEVEMENTS", "LOGROS", "CONQUISTAS", "PENCAPAIAN", "SUCCÈS", "ERFOLGE", "BAŞARIMLAR");
            Add("CHALLENGES", "CHALLENGES", "CHALLENGES", "DESAFÍOS", "DESAFIOS", "TANTANGAN", "DÉFIS", "HERAUSFORDERUNGEN", "GÖREVLER");
            Add("CUSTOMIZE", "CUSTOMIZE", "CUSTOMIZE", "PERSONALIZAR", "PERSONALIZAR", "KUSTOMISASI", "PERSONNALISER", "ANPASSEN", "ÖZELLEŞTİR");
            Add("LEADERBOARD", "LEADERBOARD", "LEADERBOARD", "CLASIFICACIÓN", "RANKING", "PERINGKAT", "CLASSEMENT", "BESTENLISTE", "SIRALAMA");
            Add("STORY", "STORY", "STORY", "HISTORIA", "HISTÓRIA", "CERITA", "HISTOIRE", "GESCHICHTE", "HİKÂYE");
        }
    }
}
