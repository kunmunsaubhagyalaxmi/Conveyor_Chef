namespace Watermelon
{
    // Words written on the text-free art (buttons, ribbons, popups).
    // Columns: English, Hinglish, Spanish, Portuguese, Indonesian, French, German, Turkish.
    public static partial class LocTable
    {
        private static void ArtWords()
        {
            Add("CONTINUE", "CONTINUE", "", "CONTINUAR", "CONTINUAR", "LANJUT", "CONTINUER", "WEITER", "DEVAM");
            Add("RESTART", "RESTART", "", "REINICIAR", "REINICIAR", "ULANGI", "RECOMMENCER", "NEUSTART", "YENİDEN");
            Add("GET X3", "GET X3", "", "OBTÉN X3", "PEGUE X3", "DAPAT X3", "OBTENIR X3", "X3 HOLEN", "X3 AL");
            Add("SOUND ON", "SOUND ON", "", "SONIDO SÍ", "SOM LIGADO", "SUARA AKTIF", "SON ACTIVÉ", "TON AN", "SES AÇIK");
            Add("SOUND OFF", "SOUND OFF", "", "SONIDO NO", "SOM DESLIGADO", "SUARA MATI", "SON COUPÉ", "TON AUS", "SES KAPALI");
            Add("MUSIC ON", "MUSIC ON", "", "MÚSICA SÍ", "MÚSICA LIGADA", "MUSIK AKTIF", "MUSIQUE ACTIVÉE", "MUSIK AN", "MÜZİK AÇIK");
            Add("MUSIC OFF", "MUSIC OFF", "", "MÚSICA NO", "MÚSICA DESLIGADA", "MUSIK MATI", "MUSIQUE COUPÉE", "MUSIK AUS", "MÜZİK KAPALI");
            Add("VIBRATION ON", "VIBRATION ON", "", "VIBRACIÓN SÍ", "VIBRAÇÃO LIGADA", "GETAR AKTIF", "VIBRATION ACTIVÉE", "VIBRATION AN", "TİTREŞİM AÇIK");
            Add("VIBRATION OFF", "VIBRATION OFF", "", "VIBRACIÓN NO", "VIBRAÇÃO DESLIGADA", "GETAR MATI", "VIBRATION COUPÉE", "VIBRATION AUS", "TİTREŞİM KAPALI");
            Add("LEVEL COMPLETED", "LEVEL COMPLETED", "", "¡NIVEL COMPLETADO!", "FASE CONCLUÍDA!", "LEVEL SELESAI!", "NIVEAU TERMINÉ !", "LEVEL GESCHAFFT!", "BÖLÜM TAMAM!");
            Add("LEVEL FAILED", "LEVEL FAILED", "", "NIVEL FALLIDO", "FASE PERDIDA", "LEVEL GAGAL", "NIVEAU ÉCHOUÉ", "LEVEL VERLOREN", "BÖLÜM BAŞARISIZ");
            Add("COUNTRY COMPLETED!", "COUNTRY COMPLETED!", "", "¡PAÍS COMPLETADO!", "PAÍS CONCLUÍDO!", "NEGARA SELESAI!", "PAYS TERMINÉ !", "LAND GESCHAFFT!", "ÜLKE TAMAM!");
            Add("NEXT COUNTRY", "NEXT COUNTRY", "", "SIGUIENTE PAÍS", "PRÓXIMO PAÍS", "NEGARA BERIKUTNYA", "PAYS SUIVANT", "NÄCHSTES LAND", "SONRAKİ ÜLKE");
            Add("You completed", "You completed", "", "Has completado", "Você completou", "Kamu menyelesaikan", "Tu as terminé", "Du hast geschafft", "Tamamladın");
            Add("Great job! {0} is complete.\nYour next destination is {1}", "Great job! {0} is complete.\nYour next destination is {1}", "",
                "¡Buen trabajo! {0} completado.\nTu próximo destino es {1}",
                "Ótimo trabalho! {0} concluído.\nSeu próximo destino é {1}",
                "Kerja bagus! {0} selesai.\nTujuan berikutnya: {1}",
                "Bravo ! {0} est terminé.\nTa prochaine destination : {1}",
                "Super gemacht! {0} ist geschafft.\nDein nächstes Ziel ist {1}",
                "Harika! {0} tamamlandı.\nSıradaki durağın {1}");
            Add("Amazing! {0} is complete.\nYou have completed every country!", "Amazing! {0} is complete.\nYou have completed every country!", "",
                "¡Increíble! {0} completado.\n¡Has completado todos los países!",
                "Incrível! {0} concluído.\nVocê completou todos os países!",
                "Luar biasa! {0} selesai.\nKamu menyelesaikan semua negara!",
                "Incroyable ! {0} est terminé.\nTu as terminé tous les pays !",
                "Fantastisch! {0} ist geschafft.\nDu hast alle Länder geschafft!",
                "Muhteşem! {0} tamamlandı.\nTüm ülkeleri tamamladın!");
        }
    }
}
