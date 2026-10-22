namespace Watermelon
{
    // Menu panel texts (Shop, Collection, Story, Leaderboard, Achievements, Challenges, Customize).
    // Columns: English, Hinglish, Spanish, Portuguese, Indonesian, French, German, Turkish.
    // Hinglish menus use the English text (""), as Hinglish games usually do.
    public static partial class LocTable
    {
        private static void Menus()
        {
            // ---- shared buttons / labels
            Add("PLAY", "PLAY", "", "JUGAR", "JOGAR", "MAIN", "JOUER", "SPIELEN", "OYNA");
            Add("REPLAY", "REPLAY", "", "REPETIR", "REJOGAR", "ULANGI", "REJOUER", "NOCHMAL", "TEKRAR");
            Add("CLAIM", "CLAIM", "", "RECLAMAR", "RESGATAR", "KLAIM", "RÉCUPÉRER", "ABHOLEN", "AL");
            Add("CLAIMED", "CLAIMED", "", "RECLAMADO", "RESGATADO", "DIKLAIM", "RÉCUPÉRÉ", "ABGEHOLT", "ALINDI");
            Add("REWARD", "REWARD", "", "PREMIO", "PRÊMIO", "HADIAH", "RÉCOMPENSE", "BELOHNUNG", "ÖDÜL");
            Add("FREE", "FREE", "", "GRATIS", "GRÁTIS", "GRATIS", "GRATUIT", "GRATIS", "BEDAVA");
            Add("BUY", "BUY", "", "COMPRAR", "COMPRAR", "BELI", "ACHETER", "KAUFEN", "SATIN AL");
            Add("SELECT", "SELECT", "", "ELEGIR", "ESCOLHER", "PILIH", "CHOISIR", "WÄHLEN", "SEÇ");
            Add("IN USE", "IN USE", "", "EN USO", "EM USO", "DIPAKAI", "UTILISÉ", "AKTIV", "KULLANILIYOR");
            Add("ACTIVE", "ACTIVE", "", "ACTIVO", "ATIVO", "AKTIF", "ACTIF", "AKTIV", "AKTİF");
            Add("TAKEN", "TAKEN", "", "RECOGIDO", "PEGO", "DIAMBIL", "PRIS", "ABGEHOLT", "ALINDI");
            Add("MASTER", "MASTER", "", "MAESTRO", "MESTRE", "MASTER", "MAÎTRE", "MEISTER", "USTA");
            Add("COMING SOON", "COMING SOON", "", "PRÓXIMAMENTE", "EM BREVE", "SEGERA HADIR", "BIENTÔT", "BALD VERFÜGBAR", "YAKINDA");
            Add("Not enough coins", "Not enough coins", "", "No tienes suficientes monedas", "Moedas insuficientes", "Koin tidak cukup", "Pas assez de pièces", "Nicht genug Münzen", "Yeterli altın yok");
            Add("Not enough diamonds", "Not enough diamonds", "", "No tienes suficientes diamantes", "Diamantes insuficientes", "Berlian tidak cukup", "Pas assez de diamants", "Nicht genug Diamanten", "Yeterli elmas yok");
            // ---- OUT OF LIVES
            Add("OUT OF LIVES", "OUT OF LIVES", "", "SIN VIDAS", "SEM VIDAS", "NYAWA HABIS", "PLUS DE VIES", "KEINE LEBEN", "CAN KALMADI");
            Add("Next life in {0}", "Next life in {0}", "", "Próxima vida en {0}", "Próxima vida em {0}", "Nyawa berikutnya dalam {0}", "Prochaine vie dans {0}", "Nächstes Leben in {0}", "Sonraki can {0} sonra");
            Add("No lives left! Wait or refill now.", "No lives left! Wait or refill now.", "", "¡Sin vidas! Espera o recarga ahora.", "Sem vidas! Espere ou recarregue agora.", "Nyawa habis! Tunggu atau isi sekarang.", "Plus de vies ! Attends ou recharge.", "Keine Leben! Warte oder fülle auf.", "Can kalmadı! Bekle ya da doldur.");
            Add("BUY LIFE", "BUY LIFE", "", "COMPRAR VIDA", "COMPRAR VIDA", "BELI NYAWA", "ACHETER VIE", "LEBEN KAUFEN", "CAN AL");
            Add("REFILL ALL", "REFILL ALL", "", "RELLENAR TODO", "RECARREGAR", "ISI PENUH", "TOUT REMPLIR", "AUFFÜLLEN", "HEPSİNİ DOLDUR");
            Add("+{0} COINS", "+{0} COINS", "", "+{0} MONEDAS", "+{0} MOEDAS", "+{0} KOIN", "+{0} PIÈCES", "+{0} MÜNZEN", "+{0} ALTIN");
            Add("+{0} DIAMONDS", "+{0} DIAMONDS", "", "+{0} DIAMANTES", "+{0} DIAMANTES", "+{0} BERLIAN", "+{0} DIAMANTS", "+{0} DIAMANTEN", "+{0} ELMAS");
            Add("{0} selected", "{0} selected", "", "{0} elegido", "{0} escolhido", "{0} dipilih", "{0} choisi", "{0} gewählt", "{0} seçildi");

            // ---- continents (tabs)
            Add("ASIA", "ASIA", "", "ASIA", "ÁSIA", "ASIA", "ASIE", "ASIEN", "ASYA");
            Add("N.AM", "N.AM", "", "N.AM", "A.NOR", "A.UTR", "AM.N", "N.AM", "K.AM");
            Add("S.AM", "S.AM", "", "S.AM", "A.SUL", "A.SEL", "AM.S", "S.AM", "G.AM");
            Add("EUR", "EUR", "", "EUR", "EUR", "EROPA", "EUR", "EUR", "AVR");
            Add("AFR", "AFR", "", "ÁFR", "ÁFR", "AFR", "AFR", "AFR", "AFR");
            Add("OCE", "OCE", "", "OCE", "OCE", "OSE", "OCÉ", "OZE", "OKY");
            Add("ANT", "ANT", "", "ANT", "ANT", "ANT", "ANT", "ANT", "ANT");

            // ---- shop
            Add("POWER-UPS", "POWER-UPS", "", "POTENCIADORES", "POWER-UPS", "POWER-UP", "BONUS", "BOOSTER", "GÜÇLENDİRİCİLER");
            Add("COINS", "COINS", "", "MONEDAS", "MOEDAS", "KOIN", "PIÈCES", "MÜNZEN", "ALTINLAR");
            Add("COIN PACKS", "COIN PACKS", "", "PAQUETES DE MONEDAS", "PACOTES DE MOEDAS", "PAKET KOIN", "PACKS DE PIÈCES", "MÜNZPAKETE", "ALTIN PAKETLERİ");
            Add("DAILY GIFT", "DAILY GIFT", "", "REGALO DIARIO", "PRESENTE DIÁRIO", "HADIAH HARIAN", "CADEAU DU JOUR", "TAGESGESCHENK", "GÜNLÜK HEDİYE");
            Add("Next gift in {0}h {1:00}m", "Next gift in {0}h {1:00}m", "", "Próximo regalo en {0}h {1:00}m", "Próximo presente em {0}h {1:00}m", "Hadiah berikutnya dalam {0}j {1:00}m", "Prochain cadeau dans {0}h {1:00}m", "Nächstes Geschenk in {0}h {1:00}m", "Sonraki hediye {0}s {1:00}d sonra");
            Add("{0} free coins every day", "{0} free coins every day", "", "{0} monedas gratis cada día", "{0} moedas grátis todo dia", "{0} koin gratis setiap hari", "{0} pièces gratuites chaque jour", "{0} Gratismünzen jeden Tag", "Her gün {0} bedava altın");
            Add("You have {0}", "You have {0}", "", "Tienes {0}", "Você tem {0}", "Kamu punya {0}", "Tu en as {0}", "Du hast {0}", "Sende {0} tane var");
            Add("UNDO", "UNDO", "", "DESHACER", "DESFAZER", "URUNGKAN", "ANNULER", "RÜCKGÄNGIG", "GERİ AL");
            Add("HINT", "HINT", "", "PISTA", "DICA", "PETUNJUK", "INDICE", "TIPP", "İPUCU");
            Add("SHUFFLE", "SHUFFLE", "", "MEZCLAR", "EMBARALHAR", "ACAK", "MÉLANGER", "MISCHEN", "KARIŞTIR");
            Add("EXTRA PLATE", "EXTRA PLATE", "", "PLATO EXTRA", "PRATO EXTRA", "PIRING EKSTRA", "ASSIETTE EN PLUS", "EXTRA-TELLER", "EKSTRA TABAK");
            Add("EXTRA PLATE!", "EXTRA PLATE!", "", "¡PLATO EXTRA!", "PRATO EXTRA!", "PIRING EKSTRA!", "ASSIETTE EN PLUS !", "EXTRA-TELLER!", "EKSTRA TABAK!");
            Add("One more spot on the tray for this level", "One more spot on the tray for this level", "Is level ke liye tray mein ek aur jagah", "Un hueco más en la bandeja en este nivel", "Mais um espaço na bandeja nesta fase", "Satu tempat lagi di nampan untuk level ini", "Une place de plus sur le plateau pour ce niveau", "Ein Platz mehr auf dem Tablett für dieses Level", "Bu bölüm için tepside bir yer daha");
            Add("Undo the last move", "Undo the last move", "", "Deshace el último movimiento", "Desfaz a última jogada", "Batalkan langkah terakhir", "Annule le dernier coup", "Macht den letzten Zug rückgängig", "Son hamleyi geri alır");
            Add("Complete pair", "Complete pair", "", "Completa un trío", "Completa um trio", "Lengkapi pasangan", "Complète un trio", "Vervollständigt ein Trio", "Bir üçlüyü tamamlar");
            Add("Shuffle characters on the map", "Shuffle the dishes on the board", "", "Mezcla los platos del tablero", "Embaralha os pratos do tabuleiro", "Acak hidangan di papan", "Mélange les plats du plateau", "Mischt die Gerichte auf dem Brett", "Tahtadaki yemekleri karıştırır");
            Add("NO ADS", "NO ADS", "", "SIN ANUNCIOS", "SEM ANÚNCIOS", "TANPA IKLAN", "SANS PUB", "KEINE WERBUNG", "REKLAMSIZ");
            Add("REMOVE ADS", "REMOVE ADS", "", "QUITAR ANUNCIOS", "REMOVER ANÚNCIOS", "HAPUS IKLAN", "SUPPRIMER LES PUB", "WERBUNG ENTFERNEN", "REKLAMLARI KALDIR");
            Add("Ads are off. Thank you!", "Ads are off. Thank you!", "", "Anuncios desactivados. ¡Gracias!", "Anúncios desativados. Obrigado!", "Iklan dimatikan. Terima kasih!", "Pubs désactivées. Merci !", "Werbung ist aus. Danke!", "Reklamlar kapalı. Teşekkürler!");
            Add("No ads between levels", "No ads between levels", "", "Sin anuncios entre niveles", "Sem anúncios entre as fases", "Tanpa iklan antar level", "Pas de pub entre les niveaux", "Keine Werbung zwischen Levels", "Bölümler arasında reklam yok");
            Add("Not available right now", "Not available right now", "", "No disponible ahora", "Indisponível no momento", "Tidak tersedia saat ini", "Indisponible pour le moment", "Gerade nicht verfügbar", "Şu anda kullanılamıyor");
            Add("Store not available. Try again later.", "Store not available. Try again later.", "", "Tienda no disponible. Inténtalo más tarde.", "Loja indisponível. Tente mais tarde.", "Toko tidak tersedia. Coba lagi nanti.", "Boutique indisponible. Réessaie plus tard.", "Shop nicht verfügbar. Versuch es später.", "Mağaza kullanılamıyor. Sonra tekrar dene.");
            Add("Ads removed!", "Ads removed!", "", "¡Anuncios eliminados!", "Anúncios removidos!", "Iklan dihapus!", "Pubs supprimées !", "Werbung entfernt!", "Reklamlar kaldırıldı!");

            // ---- collection
            Add("DISHES DISCOVERED  {0}/{1}", "DISHES DISCOVERED  {0}/{1}", "", "PLATOS DESCUBIERTOS  {0}/{1}", "PRATOS DESCOBERTOS  {0}/{1}", "HIDANGAN DITEMUKAN  {0}/{1}", "PLATS DÉCOUVERTS  {0}/{1}", "GERICHTE ENTDECKT  {0}/{1}", "KEŞFEDİLEN YEMEKLER  {0}/{1}");
            Add("Country Master badges: {0}/{1}", "Country Master badges: {0}/{1}", "", "Insignias de maestro: {0}/{1}", "Insígnias de mestre: {0}/{1}", "Lencana master negara: {0}/{1}", "Badges de maître : {0}/{1}", "Länder-Meisterabzeichen: {0}/{1}", "Ülke ustası rozetleri: {0}/{1}");
            Add("{0} MASTER", "{0} MASTER", "", "MAESTRO DE {0}", "MESTRE DE {0}", "MASTER {0}", "MAÎTRE : {0}", "{0}-MEISTER", "{0} USTASI");
            Add("Serve all {0} dishes once", "Serve all {0} dishes once", "", "Sirve los {0} platos una vez", "Sirva os {0} pratos uma vez", "Sajikan semua {0} hidangan sekali", "Sers les {0} plats une fois", "Serviere alle {0} Gerichte einmal", "{0} yemeğin hepsini bir kez servis et");
            Add("{0} / {1} served", "{0} / {1} served", "", "{0} / {1} servidos", "{0} / {1} servidos", "{0} / {1} disajikan", "{0} / {1} servis", "{0} / {1} serviert", "{0} / {1} servis edildi");
            Add("Badge earned!", "Badge earned!", "", "¡Insignia ganada!", "Insígnia conquistada!", "Lencana didapat!", "Badge obtenu !", "Abzeichen verdient!", "Rozet kazanıldı!");
            Add("{0} Master! +{1} coins, +{2} gems", "{0} Master! +{1} coins, +{2} gems", "", "¡Maestro de {0}! +{1} monedas, +{2} gemas", "Mestre de {0}! +{1} moedas, +{2} gemas", "Master {0}! +{1} koin, +{2} permata", "Maître : {0} ! +{1} pièces, +{2} gemmes", "{0}-Meister! +{1} Münzen, +{2} Edelsteine", "{0} Ustası! +{1} altın, +{2} mücevher");
            Add("Served x{0}", "Served x{0}", "", "Servido x{0}", "Servido x{0}", "Disajikan x{0}", "Servi x{0}", "Serviert x{0}", "{0} kez servis");
            Add("Discovered", "Discovered", "", "Descubierto", "Descoberto", "Ditemukan", "Découvert", "Entdeckt", "Keşfedildi");
            Add("Not found yet", "Not found yet", "", "Aún no encontrado", "Ainda não encontrado", "Belum ditemukan", "Pas encore trouvé", "Noch nicht gefunden", "Henüz bulunmadı");

            // ---- story
            Add("THE CHEF'S JOURNEY", "THE CHEF'S JOURNEY", "", "EL VIAJE DEL CHEF", "A JORNADA DO CHEF", "PERJALANAN SANG KOKI", "LE VOYAGE DU CHEF", "DIE REISE DES KOCHS", "ŞEFİN YOLCULUĞU");
            Add("Cook your way around the world and master every country's street food.", "Cook your way around the world and master every country's street food.", "",
                "Cocina por todo el mundo y domina la comida callejera de cada país.",
                "Cozinhe pelo mundo todo e domine a comida de rua de cada país.",
                "Memasaklah keliling dunia dan kuasai jajanan jalanan setiap negara.",
                "Cuisine autour du monde et maîtrise la street food de chaque pays.",
                "Koche dich um die Welt und meistere das Streetfood jedes Landes.",
                "Dünyayı yemek yaparak dolaş ve her ülkenin sokak lezzetlerinde ustalaş.");
            Add("CHAPTER {0}", "CHAPTER {0}", "", "CAPÍTULO {0}", "CAPÍTULO {0}", "BAB {0}", "CHAPITRE {0}", "KAPITEL {0}", "BÖLÜM {0}");
            Add("LEVELS {0}/{1}", "LEVELS {0}/{1}", "", "NIVELES {0}/{1}", "FASES {0}/{1}", "LEVEL {0}/{1}", "NIVEAUX {0}/{1}", "LEVELS {0}/{1}", "SEVİYE {0}/{1}");
            Add("Finish {0}", "Finish {0}", "", "Termina {0}", "Termine {0}", "Selesaikan {0}", "Termine {0}", "Schließe {0} ab", "{0} bitir");

            // ---- leaderboard
            Add("YOU", "YOU", "", "TÚ", "VOCÊ", "KAMU", "TOI", "DU", "SEN");
            Add("YOUR CHEF SCORE", "YOUR CHEF SCORE", "", "TU PUNTUACIÓN DE CHEF", "SUA PONTUAÇÃO DE CHEF", "SKOR KOKIMU", "TON SCORE DE CHEF", "DEIN KOCH-PUNKTESTAND", "ŞEF PUANIN");
            Add("RANK", "RANK", "", "PUESTO", "POSIÇÃO", "PERINGKAT", "RANG", "RANG", "SIRA");
            Add("Star +100   |   Level +50   |   Dish +2", "Star +100   |   Level +50   |   Dish +2", "",
                "Estrella +100   |   Nivel +50   |   Plato +2", "Estrela +100   |   Fase +50   |   Prato +2",
                "Bintang +100   |   Level +50   |   Hidangan +2", "Étoile +100   |   Niveau +50   |   Plat +2",
                "Stern +100   |   Level +50   |   Gericht +2", "Yıldız +100   |   Seviye +50   |   Yemek +2");

            // ---- achievements
            Add("UNLOCKED {0} / {1}", "UNLOCKED {0} / {1}", "", "DESBLOQUEADOS {0} / {1}", "DESBLOQUEADAS {0} / {1}", "TERBUKA {0} / {1}", "DÉBLOQUÉS {0} / {1}", "FREIGESCHALTET {0} / {1}", "AÇILDI {0} / {1}");
            Add("Finish goals to earn coins and diamonds", "Finish goals to earn coins and diamonds", "", "Cumple objetivos para ganar monedas y diamantes", "Cumpra metas para ganhar moedas e diamantes", "Selesaikan target untuk dapat koin dan berlian", "Atteins des objectifs pour gagner pièces et diamants", "Erfülle Ziele für Münzen und Diamanten", "Altın ve elmas kazanmak için hedefleri tamamla");
            Add("First Service", "First Service", "", "Primer servicio", "Primeiro serviço", "Layanan pertama", "Premier service", "Erster Service", "İlk servis");
            Add("Line Cook", "Line Cook", "", "Cocinero de línea", "Cozinheiro de linha", "Juru masak", "Commis de cuisine", "Postenkoch", "Hat aşçısı");
            Add("Head Chef", "Head Chef", "", "Chef principal", "Chef principal", "Kepala koki", "Chef de cuisine", "Küchenchef", "Baş şef");
            Add("Rising Star", "Rising Star", "", "Estrella emergente", "Estrela em ascensão", "Bintang baru", "Étoile montante", "Aufsteigender Stern", "Yükselen yıldız");
            Add("Star Chef", "Star Chef", "", "Chef estrella", "Chef estrela", "Koki bintang", "Chef étoilé", "Sternekoch", "Yıldız şef");
            Add("Master of Stars", "Master of Stars", "", "Maestro de estrellas", "Mestre das estrelas", "Master bintang", "Maître des étoiles", "Meister der Sterne", "Yıldızların ustası");
            Add("Busy Kitchen", "Busy Kitchen", "", "Cocina ocupada", "Cozinha agitada", "Dapur sibuk", "Cuisine animée", "Volle Küche", "Yoğun mutfak");
            Add("Food Rush", "Food Rush", "", "Fiebre culinaria", "Correria na cozinha", "Serbuan makanan", "Rush culinaire", "Essensansturm", "Yemek telaşı");
            Add("Conveyor Legend", "Conveyor Legend", "", "Leyenda de la cinta", "Lenda da esteira", "Legenda konveyor", "Légende du tapis", "Fließband-Legende", "Bant efsanesi");
            Add("Gourmet Collector", "Gourmet Collector", "", "Coleccionista gourmet", "Colecionador gourmet", "Kolektor gurme", "Collectionneur gourmet", "Gourmet-Sammler", "Gurme koleksiyoncu");
            Add("Helping Hand", "Helping Hand", "", "Mano amiga", "Mão amiga", "Tangan penolong", "Coup de main", "Helfende Hand", "Yardım eli");
            Add("Complete your first level", "Complete your first level", "", "Completa tu primer nivel", "Complete sua primeira fase", "Selesaikan level pertamamu", "Termine ton premier niveau", "Schaffe dein erstes Level", "İlk bölümünü tamamla");
            Add("Complete 5 levels", "Complete 5 levels", "", "Completa 5 niveles", "Complete 5 fases", "Selesaikan 5 level", "Termine 5 niveaux", "Schaffe 5 Levels", "5 bölüm tamamla");
            Add("Complete all {0} levels", "Complete all {0} levels", "", "Completa los {0} niveles", "Complete as {0} fases", "Selesaikan semua {0} level", "Termine les {0} niveaux", "Schaffe alle {0} Levels", "{0} bölümün hepsini tamamla");
            Add("Collect 10 stars", "Collect 10 stars", "", "Consigue 10 estrellas", "Ganhe 10 estrelas", "Kumpulkan 10 bintang", "Gagne 10 étoiles", "Sammle 10 Sterne", "10 yıldız topla");
            Add("Collect 30 stars", "Collect 30 stars", "", "Consigue 30 estrellas", "Ganhe 30 estrelas", "Kumpulkan 30 bintang", "Gagne 30 étoiles", "Sammle 30 Sterne", "30 yıldız topla");
            Add("Collect all {0} stars", "Collect all {0} stars", "", "Consigue las {0} estrellas", "Ganhe as {0} estrelas", "Kumpulkan semua {0} bintang", "Gagne les {0} étoiles", "Sammle alle {0} Sterne", "{0} yıldızın hepsini topla");
            Add("Serve 100 dishes", "Serve 100 dishes", "", "Sirve 100 platos", "Sirva 100 pratos", "Sajikan 100 hidangan", "Sers 100 plats", "Serviere 100 Gerichte", "100 yemek servis et");
            Add("Serve 500 dishes", "Serve 500 dishes", "", "Sirve 500 platos", "Sirva 500 pratos", "Sajikan 500 hidangan", "Sers 500 plats", "Serviere 500 Gerichte", "500 yemek servis et");
            Add("Serve 2,000 dishes", "Serve 2,000 dishes", "", "Sirve 2.000 platos", "Sirva 2.000 pratos", "Sajikan 2.000 hidangan", "Sers 2 000 plats", "Serviere 2.000 Gerichte", "2.000 yemek servis et");
            Add("Discover all {0} dishes", "Discover all {0} dishes", "", "Descubre los {0} platos", "Descubra os {0} pratos", "Temukan semua {0} hidangan", "Découvre les {0} plats", "Entdecke alle {0} Gerichte", "{0} yemeğin hepsini keşfet");
            Add("Use 10 power-ups", "Use 10 power-ups", "", "Usa 10 potenciadores", "Use 10 power-ups", "Pakai 10 power-up", "Utilise 10 bonus", "Nutze 10 Booster", "10 güçlendirici kullan");

            // ---- challenges
            Add("DAILY CHALLENGES", "DAILY CHALLENGES", "", "DESAFÍOS DIARIOS", "DESAFIOS DIÁRIOS", "TANTANGAN HARIAN", "DÉFIS DU JOUR", "TÄGLICHE AUFGABEN", "GÜNLÜK GÖREVLER");
            Add("New challenges in {0:00}:{1:00}:{2:00}", "New challenges in {0:00}:{1:00}:{2:00}", "", "Nuevos desafíos en {0:00}:{1:00}:{2:00}", "Novos desafios em {0:00}:{1:00}:{2:00}", "Tantangan baru dalam {0:00}:{1:00}:{2:00}", "Nouveaux défis dans {0:00}:{1:00}:{2:00}", "Neue Aufgaben in {0:00}:{1:00}:{2:00}", "Yeni görevler {0:00}:{1:00}:{2:00} sonra");
            Add("ALL-CLEAR BONUS", "ALL-CLEAR BONUS", "", "BONUS POR COMPLETAR TODO", "BÔNUS POR COMPLETAR TUDO", "BONUS SEMUA SELESAI", "BONUS TOUT TERMINÉ", "ALLES-GESCHAFFT-BONUS", "HEPSİNİ BİTİRME BONUSU");
            Add("Claim all {0} today", "Claim all {0} today", "", "Reclama los {0} hoy", "Resgate os {0} hoje", "Klaim semua {0} hari ini", "Récupère les {0} aujourd'hui", "Hol dir heute alle {0}", "Bugün {0} tanesinin hepsini al");
            Add("Win 2 levels", "Win 2 levels", "", "Gana 2 niveles", "Vença 2 fases", "Menangkan 2 level", "Gagne 2 niveaux", "Gewinne 2 Levels", "2 bölüm kazan");
            Add("Win 3 levels", "Win 3 levels", "", "Gana 3 niveles", "Vença 3 fases", "Menangkan 3 level", "Gagne 3 niveaux", "Gewinne 3 Levels", "3 bölüm kazan");
            Add("Win 5 levels", "Win 5 levels", "", "Gana 5 niveles", "Vença 5 fases", "Menangkan 5 level", "Gagne 5 niveaux", "Gewinne 5 Levels", "5 bölüm kazan");
            Add("Earn 6 stars", "Earn 6 stars", "", "Gana 6 estrellas", "Ganhe 6 estrelas", "Dapatkan 6 bintang", "Gagne 6 étoiles", "Verdiene 6 Sterne", "6 yıldız kazan");
            Add("Earn 9 stars", "Earn 9 stars", "", "Gana 9 estrellas", "Ganhe 9 estrelas", "Dapatkan 9 bintang", "Gagne 9 étoiles", "Verdiene 9 Sterne", "9 yıldız kazan");
            Add("Earn 12 stars", "Earn 12 stars", "", "Gana 12 estrellas", "Ganhe 12 estrelas", "Dapatkan 12 bintang", "Gagne 12 étoiles", "Verdiene 12 Sterne", "12 yıldız kazan");
            Add("Serve 40 dishes", "Serve 40 dishes", "", "Sirve 40 platos", "Sirva 40 pratos", "Sajikan 40 hidangan", "Sers 40 plats", "Serviere 40 Gerichte", "40 yemek servis et");
            Add("Serve 80 dishes", "Serve 80 dishes", "", "Sirve 80 platos", "Sirva 80 pratos", "Sajikan 80 hidangan", "Sers 80 plats", "Serviere 80 Gerichte", "80 yemek servis et");
            Add("Serve 150 dishes", "Serve 150 dishes", "", "Sirve 150 platos", "Sirva 150 pratos", "Sajikan 150 hidangan", "Sers 150 plats", "Serviere 150 Gerichte", "150 yemek servis et");
            Add("Use a power-up", "Use a power-up", "", "Usa un potenciador", "Use um power-up", "Pakai 1 power-up", "Utilise un bonus", "Nutze einen Booster", "Bir güçlendirici kullan");
            Add("Use 3 power-ups", "Use 3 power-ups", "", "Usa 3 potenciadores", "Use 3 power-ups", "Pakai 3 power-up", "Utilise 3 bonus", "Nutze 3 Booster", "3 güçlendirici kullan");

            // ---- customize
            Add("CHEF", "CHEF", "", "CHEF", "CHEF", "KOKI", "CHEF", "KOCH", "ŞEF");
            Add("TRAYS", "TRAYS", "", "BANDEJAS", "BANDEJAS", "NAMPAN", "PLATEAUX", "TABLETTS", "TEPSİLER");
            Add("PLATES", "PLATES", "", "PLATOS", "PRATOS", "PIRING", "ASSIETTES", "TELLER", "TABAKLAR");
            Add("LIDS", "LIDS", "", "TAPAS", "TAMPAS", "TUTUP", "CLOCHES", "DECKEL", "KAPAKLAR");
            Add("CHEF LEVEL {0}", "CHEF LEVEL {0}", "", "NIVEL DE CHEF {0}", "NÍVEL DE CHEF {0}", "LEVEL KOKI {0}", "NIVEAU DE CHEF {0}", "KOCH-STUFE {0}", "ŞEF SEVİYESİ {0}");
            Add("Next outfit: {0} at level {1}", "Next outfit: {0} at level {1}", "", "Siguiente traje: {0} en el nivel {1}", "Próxima roupa: {0} no nível {1}", "Pakaian berikutnya: {0} di level {1}", "Prochaine tenue : {0} au niveau {1}", "Nächstes Outfit: {0} auf Stufe {1}", "Sonraki kıyafet: {1}. seviyede {0}");
            Add("Every outfit unlocked!", "Every outfit unlocked!", "", "¡Todos los trajes desbloqueados!", "Todas as roupas desbloqueadas!", "Semua pakaian terbuka!", "Toutes les tenues débloquées !", "Alle Outfits freigeschaltet!", "Tüm kıyafetler açıldı!");
            Add("Win levels to earn XP: +{0} XP per win", "Win levels to earn XP: +{0} XP per win", "", "Gana niveles para conseguir XP: +{0} XP por victoria", "Vença fases para ganhar XP: +{0} XP por vitória", "Menangkan level untuk XP: +{0} XP per kemenangan", "Gagne des niveaux pour l'XP : +{0} XP par victoire", "Gewinne Levels für XP: +{0} XP pro Sieg", "XP için bölüm kazan: galibiyet başına +{0} XP");
            Add("No trays available yet", "No trays available yet", "", "Aún no hay bandejas", "Ainda não há bandejas", "Belum ada nampan", "Pas encore de plateaux", "Noch keine Tabletts", "Henüz tepsi yok");
            Add("The glass lid covers dishes you cannot pick yet.", "The glass lid covers dishes you cannot pick yet.", "", "La tapa de cristal cubre los platos que aún no puedes coger.", "A tampa de vidro cobre os pratos que você ainda não pode pegar.", "Tutup kaca menutupi hidangan yang belum bisa diambil.", "La cloche en verre couvre les plats que tu ne peux pas encore prendre.", "Der Glasdeckel deckt Gerichte ab, die du noch nicht nehmen kannst.", "Cam kapak henüz alamadığın yemekleri örter.");
            Add("Plates always keep their tray colour.", "Plates always keep their tray colour.", "", "Los platos siempre llevan el color de su bandeja.", "Os pratos sempre têm a cor da bandeja.", "Piring selalu mengikuti warna nampannya.", "Les assiettes gardent la couleur de leur plateau.", "Teller behalten immer die Farbe ihres Tabletts.", "Tabaklar her zaman tepsilerinin rengindedir.");
            Add("Classic Chef", "Classic Chef", "", "Chef clásico", "Chef clássico", "Koki klasik", "Chef classique", "Klassischer Koch", "Klasik şef");
            Add("Street Food Chef", "Street Food Chef", "", "Chef callejero", "Chef de rua", "Koki jajanan", "Chef street food", "Streetfood-Koch", "Sokak lezzetleri şefi");
            Add("Pastry Chef", "Pastry Chef", "", "Chef pastelero", "Confeiteiro", "Koki kue", "Chef pâtissier", "Konditor", "Pasta şefi");
            Add("Sushi Master", "Sushi Master", "", "Maestro del sushi", "Mestre do sushi", "Master sushi", "Maître sushi", "Sushi-Meister", "Suşi ustası");
            Add("Pizza Maestro", "Pizza Maestro", "", "Maestro pizzero", "Mestre pizzaiolo", "Maestro pizza", "Maestro de la pizza", "Pizza-Maestro", "Pizza ustası");
            Add("BBQ Pitmaster", "BBQ Pitmaster", "", "Maestro parrillero", "Mestre churrasqueiro", "Master BBQ", "Maître du barbecue", "Grillmeister", "Mangal ustası");
            Add("Royal Chef", "Royal Chef", "", "Chef real", "Chef real", "Koki kerajaan", "Chef royal", "Königlicher Koch", "Saray şefi");
            Add("Golden Chef", "Golden Chef", "", "Chef dorado", "Chef dourado", "Koki emas", "Chef doré", "Goldener Koch", "Altın şef");
            Add("The trusty white jacket", "The trusty white jacket", "", "La fiel chaqueta blanca", "O fiel dólmã branco", "Jaket putih andalan", "La fidèle veste blanche", "Die treue weiße Jacke", "Güvenilir beyaz ceket");
            Add("Bandana and a busy apron", "Bandana and a busy apron", "", "Pañuelo y un delantal muy usado", "Bandana e um avental de batalha", "Bandana dan celemek sibuk", "Bandana et tablier bien rempli", "Bandana und eine fleißige Schürze", "Bandana ve hareketli bir önlük");
            Add("Pink apron dusted with flour", "Pink apron dusted with flour", "", "Delantal rosa con harina", "Avental rosa sujo de farinha", "Celemek merah muda bertabur tepung", "Tablier rose saupoudré de farine", "Rosa Schürze voller Mehl", "Una bulanmış pembe önlük");
            Add("Hachimaki headband and a sharp knife", "Hachimaki headband and a sharp knife", "", "Cinta hachimaki y un cuchillo afilado", "Faixa hachimaki e uma faca afiada", "Ikat kepala hachimaki dan pisau tajam", "Bandeau hachimaki et couteau aiguisé", "Hachimaki-Stirnband und ein scharfes Messer", "Hachimaki bandı ve keskin bir bıçak");
            Add("Red scarf and a pizza peel", "Red scarf and a pizza peel", "", "Pañuelo rojo y una pala de pizza", "Lenço vermelho e uma pá de pizza", "Syal merah dan sekop pizza", "Foulard rouge et pelle à pizza", "Rotes Halstuch und ein Pizzaschieber", "Kırmızı fular ve pizza küreği");
            Add("Leather apron and big tongs", "Leather apron and big tongs", "", "Delantal de cuero y pinzas grandes", "Avental de couro e pegador grande", "Celemek kulit dan penjepit besar", "Tablier en cuir et grandes pinces", "Lederschürze und große Zange", "Deri önlük ve büyük maşa");
            Add("Gold-trimmed jacket fit for a palace", "Gold-trimmed jacket fit for a palace", "", "Chaqueta con bordes dorados digna de un palacio", "Dólmã com detalhes dourados digno de um palácio", "Jaket berlis emas untuk istana", "Veste brodée d'or digne d'un palais", "Goldbesetzte Jacke für den Palast", "Saraya yakışır altın işlemeli ceket");
            Add("A legend in a shining gold jacket", "A legend in a shining gold jacket", "", "Una leyenda con chaqueta dorada brillante", "Uma lenda num dólmã dourado brilhante", "Legenda berjaket emas berkilau", "Une légende en veste dorée", "Eine Legende in glänzender Goldjacke", "Parlak altın ceketli bir efsane");
        }
    }
}
