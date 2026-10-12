"""Writes the complete list of Level Selection card pictures (30 per country).

    python card_art_list.py

Output (Docs/LevelCardArt/):
  card_art_list.csv  - one row per card: continent, country, level, file, title, status, prompt
  card_art_list.md   - the same, readable, grouped by country, with the counts

Status: EXISTS (already in the project), COPY (exists elsewhere in the project, will be copied),
GENERATE (picture to be made). Titles of levels 1-3 are the country's mission names in
WorldCatalog.cs; levels 4-30 use the place names below.
"""
import csv
import os
import re

HERE = os.path.dirname(os.path.abspath(__file__))
PROJECT = os.path.normpath(os.path.join(HERE, '..', '..'))
CATALOG = os.path.join(PROJECT, 'Assets', 'Project Data', 'Game', 'Scripts', 'World', 'WorldCatalog.cs')
WORLD = os.path.join(PROJECT, 'Assets', 'Project Data', 'Game', 'Resources', 'World')
OUT = os.path.join(PROJECT, 'Docs', 'LevelCardArt')

# Places for levels 4-30 of every country (27 each), in level order.
PLACES = {
    'china': ['Great Wall Snacks', 'Forbidden City', 'Temple of Heaven', 'Summer Palace', "Xi'an Muslim Quarter",
              'Terracotta Army', 'Chengdu Panda Cafe', 'Hangzhou West Lake', 'Suzhou Canals', 'Guilin River Rafts',
              'Yangshuo Peaks', 'Hong Kong Harbour', 'Macau Streets', 'Guangzhou Dim Sum', 'Harbin Ice Festival',
              'Qingdao Pier', 'Lhasa Tea House', 'Yunnan Rice Terraces', 'Zhangjiajie Peaks', 'Wuhan Breakfast Lane',
              'Nanjing Night Market', 'Chongqing Hot Pot', 'Lijiang Old Town', 'Huangshan Tea Hills', 'Kaifeng Night Fair',
              'Shenzhen Skyline', 'Hainan Beach Grill'],
    'japan': ['Shibuya Crossing', 'Asakusa Temple', 'Tsukiji Fish Market', 'Mount Fuji Lodge', 'Nara Deer Park',
              'Hiroshima Okonomiyaki', 'Sapporo Snow Fest', 'Hakone Hot Springs', 'Nikko Shrines', 'Kobe Beef Grill',
              'Okinawa Beach Bar', 'Fukuoka Yatai', 'Arashiyama Bamboo', 'Fushimi Inari', 'Kanazawa Market',
              'Yokohama Chinatown', 'Nagoya Castle', 'Takayama Old Town', 'Shirakawa-go', 'Miyajima Shrine',
              'Kamakura Buddha', 'Akihabara Lights', 'Gion Tea House', 'Sendai Tanabata', 'Harajuku Sweets',
              'Ginza Sushi Bar', 'Himeji Castle'],
    'india': ['Taj Mahal Terrace', 'Jaipur Pink City', 'Jaisalmer Desert Camp', 'Udaipur Lake Palace', 'Goa Beach Shack',
              'Kerala Backwaters', 'Munnar Tea Gardens', 'Kolkata Victoria Hall', 'Chennai Marina', 'Hyderabad Charminar',
              'Amritsar Golden Temple', 'Rishikesh Ganga', 'Darjeeling Hills', 'Mysore Palace', 'Hampi Ruins',
              'Pune Food Street', 'Lucknow Kebab Lane', 'Kochi Fishing Nets', 'Ladakh Monastery', 'Shimla Mall Road',
              'Agra Petha Bazaar', 'Ahmedabad Night Market', 'Pondicherry Cafe', 'Bengaluru Darshini', 'Kanyakumari Shore',
              'Jodhpur Blue City', 'Sundarbans Village'],
    'south_korea': ['Gyeongbokgung Palace', 'Myeongdong Market', 'Gwangjang Market', 'Bukchon Hanok', 'N Seoul Tower',
                    'Jeju Island Cafe', 'Seongsan Sunrise', 'Gyeongju Temples', 'Haeundae Beach', 'Jagalchi Fish Market',
                    'Insadong Tea House', 'Hongdae Nights', 'Itaewon Grill', 'Suwon Fortress', 'Andong Village',
                    'Sokcho Seafood', 'Seoraksan Lodge', 'Daegu Night Fair', 'Gamcheon Village', 'Incheon Chinatown',
                    'Boseong Tea Fields', 'Nami Island', 'Lotte Tower View', 'Dongdaemun Market', 'Chuncheon Dakgalbi',
                    'Gangneung Coffee St', 'Namsan Park'],
    'thailand': ['Grand Palace', 'Wat Arun Riverside', 'Khao San Road', 'Chatuchak Market', 'Floating Market',
                 'Ayutthaya Temples', 'Sukhothai Ruins', 'Chiang Rai Temple', 'Pai Mountain Cafe', 'Krabi Cliffs',
                 'Phi Phi Islands', 'Koh Samui Beach', 'Hua Hin Night Market', 'Pattaya Seafood', 'Kanchanaburi Bridge',
                 'Railay Beach', 'Koh Tao Diving', 'Yaowarat Chinatown', 'Lumpini Park', 'Doi Suthep Temple',
                 'Khao Yai Lodge', 'Isan Village', 'Koh Lanta Sunset', 'Mae Klong Railway', 'Asiatique Riverfront',
                 'Erawan Falls', 'Similan Islands'],
    'usa': ['Times Square', 'Central Park Cart', 'Brooklyn Pizza', 'Chicago Deep Dish', 'New Orleans Jazz',
            'Nashville Hot Chicken', 'Memphis Ribs', 'Miami Beach Cafe', 'Florida Keys', 'Las Vegas Strip',
            'Grand Canyon Camp', 'San Francisco Bay', 'Seattle Pike Market', 'Hollywood Diner', 'Route 66 Stop',
            'Boston Clam Shack', 'Philly Cheesesteak', 'Maine Lobster Pier', 'Hawaii Luau', 'Alaska Salmon Camp',
            'Yellowstone Lodge', 'Kansas City BBQ', 'Santa Fe Market', 'Washington Mall', 'Napa Valley',
            'Detroit Coney', 'Golden Gate View'],
    'mexico': ['Chichen Itza', 'Tulum Ruins', 'Guadalajara Plaza', 'Puebla Kitchen', 'Teotihuacan',
               'Xochimilco Boats', 'Coyoacan Blue House', 'Guanajuato Alleys', 'San Miguel Plaza', 'Puerto Vallarta',
               'Cabo Marina', 'Merida Market', 'Playa del Carmen', 'Cozumel Reef', 'Monterrey Grill',
               'Tijuana Street Food', 'Veracruz Port', 'Copper Canyon', 'Taxco Silver Town', 'Acapulco Cliffs',
               'Zacatecas Fair', 'Chiapas Jungle', 'Valladolid Cenote', 'Mazatlan Malecon', 'Tequila Town',
               'Day of the Dead', 'Palenque Ruins'],
    'canada': ['Niagara Falls', 'CN Tower View', 'Quebec Old City', 'Banff Lodge', 'Lake Louise',
               'Jasper Cabin', 'Whistler Village', 'Victoria Inner Harbour', 'Ottawa Parliament', 'Calgary Stampede',
               'Halifax Waterfront', 'PEI Lobster Shack', 'Cape Breton Trail', 'Churchill Polar Bears', 'Yukon Gold Camp',
               'Okanagan Orchards', 'Muskoka Lakes', "St. John's Harbour", 'Kensington Market', 'St. Lawrence Market',
               'Granville Island', 'Maple Sugar Shack', 'Tofino Surf Cafe', 'Thousand Islands', 'Edmonton Fair',
               'Winnipeg Forks', 'Montreal Bagels'],
    'cuba': ['Old Havana Square', 'Malecon Sunset', 'Vinales Valley', 'Cienfuegos Bay', 'Santiago Carnival',
             'Camaguey Streets', 'Baracoa Cocoa', 'Cayo Coco Beach', 'Cayo Largo', 'Bay of Pigs',
             'Santa Clara Plaza', 'Matanzas Bridges', 'Holguin Hills', 'Pinar Tobacco Farm', 'Topes de Collantes',
             'Havana Cigar Shop', 'Classic Car Ride', 'Capitolio View', 'Morro Castle', 'Plaza de Armas',
             'Bodeguita Bar', 'Rumba Street Party', 'Sugar Mill Valley', 'Cayo Santa Maria', 'Guardalavaca Beach',
             'Isla de la Juventud', 'Gibara Seaside'],
    'jamaica': ['Negril Cliffs', 'Ocho Rios', "Dunn's River Falls", 'Port Antonio', 'Treasure Beach',
                'Bob Marley Museum', 'Rose Hall', "Rick's Cafe", 'Boston Bay Jerk', 'Black River Safari',
                'YS Falls', 'Blue Lagoon', 'Seven Mile Beach', 'Port Royal', 'Falmouth Market',
                'Mayfield Falls', "Frenchman's Cove", 'Discovery Bay', 'Runaway Bay', 'Bamboo Avenue',
                'Coffee Farm Hills', 'Reggae Festival', 'Devon House', 'Lime Cay', 'Hellshire Fish Fry',
                'Coronation Market', 'Martha Brae Rafts'],
    'brazil': ['Christ the Redeemer', 'Sugarloaf Mountain', 'Copacabana', 'Ipanema Sunset', 'Iguacu Falls',
               'Amazon Riverboat', 'Manaus Opera', 'Pantanal Ranch', 'Ouro Preto', 'Paraty Harbour',
               'Florianopolis', 'Recife Seafront', 'Fortaleza Dunes', 'Lencois Lagoons', 'Brasilia Plaza',
               'Belem Market', 'Carnival Parade', 'Santa Teresa Tram', 'Lapa Steps', 'Buzios Beach',
               'Fernando de Noronha', 'Gramado Chocolate', 'Porto Alegre Grill', 'Ilha Grande', 'Natal Dunes',
               'Olinda Hills', 'Curitiba Gardens'],
    'argentina': ['La Boca Streets', 'San Telmo Tango', 'Recoleta Cafe', 'Puerto Madero', 'Iguazu Falls',
                  'Perito Moreno Glacier', 'El Calafate', 'Bariloche Chocolate', 'Ushuaia Port', 'Salta Market',
                  'Quebrada Colors', 'Cordoba Plaza', 'Valdes Peninsula', 'El Chalten Trail', 'Tierra del Fuego',
                  'Cafayate Wineries', 'Mar del Plata', 'Rosario Riverside', 'Tigre Delta', 'Estancia Asado',
                  'Purmamarca Hills', 'Aconcagua Base', 'Ruta 40 Stop', 'Palermo Food Hall', 'Obelisco Avenue',
                  'Jujuy Fair', 'Talampaya Canyon'],
    'peru': ['Sacred Valley', 'Rainbow Mountain', 'Lake Titicaca', 'Arequipa Plaza', 'Colca Canyon',
             'Nazca Lines', 'Paracas Bay', 'Huacachina Oasis', 'Iquitos Amazon', 'Miraflores Cliffs',
             'Barranco Bridge', 'Pisac Market', 'Ollantaytambo', 'Moray Terraces', 'Maras Salt Pans',
             'Trujillo Ruins', 'Chan Chan', 'Mancora Beach', 'Huaraz Peaks', 'Uros Floating Islands',
             'Ayacucho Crafts', 'Cajamarca Valley', 'Kuelap Fortress', 'Lima Night Market', 'Inca Trail Camp',
             'Chinchero Weavers', 'Pachacamac'],
    'colombia': ['Coffee Triangle', 'Cocora Valley', 'Salento Village', 'Guatape Rock', 'Tayrona Park',
                 'Santa Marta Bay', 'Cali Salsa Club', 'San Andres Island', 'Villa de Leyva', 'Zipaquira Salt Mine',
                 'Monserrate Peak', 'La Candelaria', 'Comuna 13', 'Barichara Town', 'Cano Cristales',
                 'Leticia Amazon', 'Rosario Islands', 'Popayan White City', 'Minca Hills', 'Palomino Beach',
                 'Lost City Trek', 'Bazurto Market', 'Getsemani Murals', 'Chicamocha Canyon', 'Paloquemao Market',
                 'Medellin Flower Fair', 'Barranquilla Carnival'],
    'chile': ['Easter Island', 'Torres del Paine', 'Chiloe Stilt Houses', 'Puerto Varas', 'Vina del Mar',
              'Pucon Volcano', 'Elqui Valley', 'Colchagua Wine', 'San Pedro Oasis', 'Moon Valley',
              'Punta Arenas', 'Marble Caves', 'Iquique Coast', 'La Serena Beach', 'Concepcion Port',
              'Valdivia River', 'Osorno Lodge', 'Frutillar Lake', 'Cajon del Maipo', 'Isla Negra',
              'Pisco Elqui', 'Arica Shore', 'Puerto Natales', 'Carretera Austral', 'Lauca Park',
              'Cerro San Cristobal', 'Mercado Central'],
    'italy': ['Florence Duomo', 'Tuscan Vineyard', 'Amalfi Coast', 'Cinque Terre', 'Lake Como',
              'Milan Galleria', 'Pisa Tower', 'Sicily Cannoli', 'Bologna Market', 'Verona Arena',
              'Positano Steps', 'Capri Grotto', 'Siena Piazza', 'Dolomites Hut', 'Sardinia Beach',
              'Turin Chocolate', 'Genoa Pesto Port', 'Parma Cheese Farm', 'Matera Caves', 'Puglia Olive Grove',
              "St. Peter's Square", 'Trevi Fountain', 'Murano Glass', 'Lake Garda', 'San Gimignano',
              'Taormina Theatre', 'Portofino Bay'],
    'france': ['Eiffel Tower Cafe', 'Montmartre Easel', 'Louvre Courtyard', 'Seine Riverboat', 'Versailles Gardens',
               'Mont Saint-Michel', 'Nice Promenade', 'Cannes Harbour', 'Bordeaux Vineyard', 'Champagne Cellars',
               'Loire Chateau', 'Normandy Cider Farm', 'Brittany Crepes', 'Alsace Village', 'Chamonix Chalet',
               'Marseille Port', 'Saint-Tropez', 'Annecy Lake', 'Avignon Bridge', 'Lavender Fields',
               'Carcassonne Walls', 'Biarritz Surf', 'Corsica Cliffs', 'Dijon Mustard Shop', 'Toulouse Market',
               'Strasbourg Fair', 'Arc de Triomphe'],
    'spain': ['Sagrada Familia', 'Park Guell', 'Las Ramblas', 'Seville Plaza', 'Alhambra Palace',
              'Cordoba Mosque', 'Granada Tapas', 'Toledo Old Town', 'San Sebastian Pintxos', 'Bilbao Riverside',
              'Ibiza Sunset', 'Mallorca Cove', 'Canary Islands', 'Malaga Beach Bar', 'Ronda Bridge',
              'Cadiz Seafood', 'Salamanca Square', 'Santiago Pilgrim Road', 'Segovia Aqueduct', 'Tenerife Volcano',
              'La Tomatina', 'Rioja Vineyard', 'Costa Brava', 'Jerez Bodega', 'Cuenca Hanging Houses',
              'Picos de Europa', 'Retiro Park'],
    'greece': ['Acropolis View', 'Plaka Streets', 'Mykonos Windmills', 'Little Venice', 'Meteora Monasteries',
               'Delphi Ruins', 'Rhodes Old Town', 'Corfu Old Fort', 'Zakynthos Shipwreck', 'Naxos Market',
               'Paros Harbour', 'Thessaloniki Port', 'Olympia Ruins', 'Nafplio Castle', 'Hydra Island',
               'Milos Coves', 'Chania Lighthouse', 'Monastiraki Market', 'Kefalonia Bay', 'Mount Olympus',
               'Oia Sunset', 'Lefkada Beach', 'Patmos Chora', 'Aegina Pistachio', 'Spetses Harbour',
               'Halkidiki Shore', 'Mystras Hills'],
    'germany': ['Brandenburg Gate', 'Neuschwanstein', 'Rhine Valley', 'Cologne Cathedral', 'Hamburg Harbour',
                'Heidelberg Castle', 'Rothenburg Town', 'Dresden Old Town', 'Nuremberg Market', 'Frankfurt Skyline',
                'Bavarian Alps', 'Lake Constance', 'Leipzig Cafe', 'Bremen Square', 'Moselle Vineyard',
                'Christmas Market', 'Berlin Wall Art', 'Potsdam Palace', 'Stuttgart Car Museum', 'Baltic Sea Pier',
                'Harz Mountains', 'Saxon Switzerland', 'Dusseldorf Altstadt', 'Wurzburg Residence', 'Bamberg Brewery',
                'Lubeck Marzipan', 'Zugspitze Summit'],
    'morocco': ['Jemaa el-Fnaa', 'Majorelle Garden', 'Fes Medina', 'Fes Tanneries', 'Casablanca Mosque',
                'Rabat Kasbah', 'Essaouira Port', 'Atlas Mountains', 'Ait Benhaddou', 'Ouarzazate Studio',
                'Todra Gorge', 'Dades Valley', 'Merzouga Dunes', 'Agadir Beach', 'Tangier Cafe',
                'Meknes Gate', 'Volubilis Ruins', 'Ifrane Village', 'Tetouan Medina', 'Asilah Murals',
                'Ouzoud Falls', 'Taroudant Walls', 'Mint Tea House', 'Spice Square', 'Argan Grove',
                'Imlil Trek', 'Rissani Market'],
    'egypt': ['Sphinx View', 'Khan el-Khalili', 'Egyptian Museum', 'Aswan Felucca', 'Abu Simbel',
              'Karnak Temple', 'Valley of the Kings', 'Alexandria Corniche', 'Red Sea Reef', 'Sharm Beach',
              'Hurghada Marina', 'Siwa Oasis', 'White Desert Camp', 'Dahab Lagoon', 'Mount Sinai',
              'Philae Temple', 'Nubian Village', 'Edfu Temple', 'Kom Ombo', 'Saqqara Pyramid',
              'Cairo Citadel', 'Al-Azhar Park', 'Nile Dinner Cruise', 'Fayoum Lake', 'Marsa Alam',
              'Rosetta Old Town', 'Port Said Canal'],
    'nigeria': ['Lekki Beach', 'Balogun Market', 'Victoria Island', 'Zuma Rock', 'Olumo Rock',
                'Yankari Safari', 'Obudu Ranch', 'Erin Ijesha Falls', 'Kano Dye Pits', 'Kano Old City',
                'Ibadan Hills', 'Benin Bronze Market', 'Osun Sacred Grove', 'Jos Plateau', 'Badagry Coast',
                'Port Harcourt Grill', 'Enugu Hills', 'Ikogosi Springs', 'Owerri Food Street', 'Argungu Fish Fest',
                'Calabar Carnival', 'Idanre Hills', 'Gurara Falls', 'Oshogbo Art Village', 'Suya Night Spot',
                'Makoko Waterfront', 'Abeokuta Market'],
    'ethiopia': ['Simien Mountains', 'Lake Tana', 'Blue Nile Falls', 'Gondar Castles', 'Axum Obelisks',
                 'Danakil Depression', 'Harar Old Town', 'Omo Valley', 'Bale Mountains', 'Merkato Market',
                 'Entoto Hills', 'Lake Hawassa', 'Arba Minch', 'Dire Dawa Market', 'Debre Damo',
                 'Tigray Rock Churches', 'Sof Omar Caves', 'Yirgacheffe Farms', 'Awash Park', 'Bahir Dar Shore',
                 'Wenchi Crater', 'Konso Terraces', 'Lake Langano', 'Jimma Coffee', 'Mekele Plaza',
                 'Sodo Village', 'Erta Ale Volcano'],
    'south_africa': ['Table Mountain', 'V&A Waterfront', 'Cape Point', 'Boulders Penguins', 'Stellenbosch Wine',
                     'Garden Route', 'Knysna Lagoon', 'Kruger Park', 'Durban Curry', 'Soweto Streets',
                     'Drakensberg Hike', 'Blyde Canyon', 'Hermanus Whales', 'Robben Island', 'Bo-Kaap Colours',
                     'Franschhoek Valley', 'Oudtshoorn Ostrich', 'Addo Elephants', 'Gqeberha Bay', 'Sun City Resort',
                     'Wild Coast', 'Tsitsikamma Bridge', 'Pilanesberg Park', 'Kalahari Camp', 'Muizenberg Beach',
                     'Constitution Hill', 'iSimangaliso Bay'],
    'australia': ['Opera House View', 'Bondi Beach', 'Uluru Sunset', 'Melbourne Laneways', 'Great Ocean Road',
                  'Twelve Apostles', 'Blue Mountains', 'Daintree Forest', 'Whitsunday Islands', 'Gold Coast',
                  'Byron Bay', 'Kakadu Park', 'Perth Beach', 'Fremantle Market', 'Margaret River',
                  'Barossa Valley', 'Kangaroo Island', 'Hobart Harbour', 'Cradle Mountain', 'Brisbane River',
                  'Cairns Esplanade', 'Noosa Heads', 'Darwin Night Market', 'Alice Springs', 'Rottnest Island',
                  'Coober Pedy', 'Queen Victoria Market'],
    'new_zealand': ['Milford Sound', 'Hobbiton Village', 'Mount Cook', 'Lake Tekapo', 'Bay of Islands',
                    'Wellington Waterfront', 'Waitomo Glowworms', 'Franz Josef Glacier', 'Abel Tasman', 'Kaikoura Whales',
                    'Napier Art Deco', 'Coromandel Beach', 'Wanaka Lake', 'Christchurch Trams', 'Dunedin Castle',
                    'Marlborough Wine', 'Tongariro Crossing', 'Cathedral Cove', 'Hot Water Beach', 'Lake Taupo',
                    'Fiordland Lodge', 'Akaroa Harbour', 'Stewart Island', 'Raglan Surf', 'Nelson Market',
                    'Matakana Market', 'Piha Beach'],
    'fiji': ['Denarau Marina', 'Mamanuca Islands', 'Yasawa Beach', 'Cloud 9 Platform', 'Taveuni Rainforest',
             'Bouma Falls', 'Savusavu Bay', 'Nadi Temple', 'Sabeto Mud Pools', 'Garden of the Giant',
             'Pacific Harbour', 'Beqa Lagoon', 'Navua River', 'Sigatoka Dunes', 'Kadavu Reef',
             'Levuka Town', 'Natadola Beach', 'Monuriki Island', 'Robinson Crusoe Island', 'Vanua Levu Coast',
             'Kava Ceremony', 'Firewalking Village', 'Rainbow Reef', 'Lautoka Market', 'Malolo Island',
             'Treasure Island', 'Namosi Highlands'],
    'samoa': ['Lalomanu Beach', 'Piula Cave Pool', 'Papaseea Sliding Rocks', 'Sopoaga Falls', 'Saleaula Lava Fields',
              'Alofaaga Blowholes', "Savai'i Coast", 'Manono Island', 'Namua Island', 'Falealupo Point',
              'Afu Aau Falls', 'Stevenson Villa', 'Fagaloa Bay', 'Poutasi Village', 'Siumu Beach',
              'Matareva Beach', 'Mount Vaea', 'Palolo Deep', 'Salamumu Shore', 'Aleipata Islands',
              'Return to Paradise', 'Lano Beach', "Pe'ape'a Cave", 'Canopy Walk', 'Apia Fish Market',
              'Fiafia Night', 'Coconut Grove'],
    'tonga': ["Vava'u Harbour", 'Swallows Cave', 'Mapu Vaea Blowholes', "Ha'amonga Trilithon", 'Royal Palace',
              'Talamahu Market', 'Pangaimotu Island', "'Eua Cliffs", 'Whale Watch Bay', 'Anahulu Cave',
              'Hufangalupe Arch', 'Oholei Beach', 'Fafa Island', 'Port of Refuge', "Mariner's Cave",
              'Neiafu Waterfront', 'Uoleva Beach', 'Lifuka Village', 'Niuatoputapu', 'Atata Island',
              'Keleti Beach', 'Tapa Cloth Village', 'Coral Garden', 'Sunset Fishing', 'Kava Circle',
              'Church Feast', 'Sea Turtle Cove'],
    'peninsula': ['Port Lockroy', 'Deception Island', 'Neko Harbour', 'Cuverville Island', 'Danco Coast',
                  'Petermann Island', 'Half Moon Island', 'Hope Bay', 'Brown Station', 'Vernadsky Base',
                  'Pleneau Bay', 'Wilhelmina Bay', 'Orne Harbour', 'Gerlache Strait', 'Iceberg Graveyard',
                  'Whalers Bay', 'Telefon Bay', 'Brown Bluff', 'Elephant Island', 'King George Island',
                  'Danger Islands', 'Antarctic Sound', 'Detaille Island', 'Crystal Sound', 'Marguerite Bay',
                  'Rothera Station', 'Adelaide Island'],
    'weddell_sea': ['Snow Hill Island', 'Filchner Ice Shelf', 'Halley Station', 'Ronne Ice Shelf', 'Larsen Ice Shelf',
                    'Seymour Island', 'Paulet Island', 'Endurance Wreck Site', 'Vahsel Bay', 'Coats Land',
                    'Atka Bay', 'Neumayer Station', 'Berkner Island', 'Gould Bay', 'Drift Ice Floe',
                    'Seal Haul-out', 'Polar Night Camp', 'Aurora Ice Field', 'Krill Bloom Bay', 'Weddell Seal Hole',
                    'Icebreaker Deck', 'Dog Sled Trail', 'Brunt Ice Shelf', 'Princess Martha Coast', 'Tabular Iceberg',
                    'Penguin Huddle', 'Blizzard Hut'],
    'ross_shelf': ['Ross Ice Front', 'Cape Royds', 'Cape Evans Hut', 'Discovery Hut', 'Hut Point',
                   'Beardmore Glacier', 'Roosevelt Island', 'Bay of Whales', 'Cape Crozier', 'Cape Adare',
                   'Franklin Island', 'Coulman Island', 'Terra Nova Bay', 'Drygalski Ice Tongue', 'Zucchelli Station',
                   'Ross Sea Orcas', 'Ice Cave Kitchen', 'Crevasse Field', 'Sastrugi Plain', 'Little America',
                   'Shackleton Inlet', 'Byrd Glacier', 'Mill Glacier', 'Siple Coast', 'Kamb Ice Stream',
                   'King Edward VII Land', 'Ross Island Ridge'],
    'mcmurdo': ['Mount Erebus', 'Observation Hill', 'Scott Base', 'Ice Runway', 'Dry Valleys',
                'Lake Vanda', 'Blood Falls', 'Taylor Valley', 'Castle Rock', 'Crary Lab',
                'Chapel of the Snows', 'Winter Quarters', 'Pegasus Field', 'Williams Field', 'Hut Point Ridge',
                'Arrival Heights', 'Cape Armitage', 'Ice Pier', 'Fuel Farm', "Gallagher's Pub",
                'Greenhouse Garden', 'Midwinter Feast', 'Aurora Night', 'Penguin Ranch', 'Seal Camp',
                'Marble Point', 'Herbie Alley'],
    'south_pole': ['Ceremonial Pole', 'Geographic Pole', 'Polar Plateau', 'IceCube Lab', 'South Pole Telescope',
                   'Dark Sector', 'Skiway Camp', 'Clean Air Sector', 'Old Dome', 'Vostok Station',
                   'Pole of Cold', 'Dome A Summit', 'Dome C Ridge', 'Concordia Base', 'Kohnen Station',
                   "Amundsen's Tent", "Scott's Last Camp", 'Polar Sauna Night', 'Midnight Sun Feast', 'Polar Aurora',
                   'Snow Mine Kitchen', 'Sledge Trail', 'Traverse Train', 'Wind Scoop Ridge', 'Polar Greenhouse',
                   'Christmas at the Pole', 'Summit Camp'],
}

# India's first three pictures already exist in Images/LevelSelection (copied, not generated)
INDIA_EXISTING = {1: 'sunset_serenity_along_the_varanasi_ghats.png',
                  2: 'delhi_street_festival_at_india_gate.png',
                  3: 'mumbai_gateway_waterfront_adventure.png'}

STYLE = ('Vibrant painterly mobile-game illustration, 1024x768 (4:3 landscape). Background: {place}, {country}, '
         'with its famous landmark clearly recognisable. Foreground: a cosy food stall or table full of {country} '
         'dishes ({dishes}), steam rising. Golden-hour or warm twilight light, glowing lanterns, rich saturated '
         'colours, soft depth of field, same style as the existing level cards. No text, no logos, no UI, '
         'no close-up people.')
POLAR = ('Vibrant painterly mobile-game illustration, 1024x768 (4:3 landscape). Background: {place}, {country} '
         'region of Antarctica - ice, snow, glaciers, penguins or seals where fitting, polar station details. '
         'Foreground: a warm explorers\' food table or camp kitchen with hot dishes ({dishes}), steam rising. '
         'Blue-hour or aurora light with warm lantern glow, rich colours, same style as the existing level cards. '
         'No text, no logos, no UI, no close-up people.')


def read_catalog():
    src = open(CATALOG, encoding='utf-8').read()
    conts = {int(i): name for i, _, name in re.findall(r'new Continent\((\d+), "(\w+)", "([^"]+)"', src)}
    cont_ids = {int(i): cid for i, cid, _ in re.findall(r'new Continent\((\d+), "(\w+)", "([^"]+)"', src)}
    out = []
    for m in re.finditer(r'new Country\((\d+), "(\w+)", "([^"]+)",.*?new\[\] \{([^}]*)\},\s*new\[\] \{([^}]*)\}\)', src, re.S):
        i = int(m.group(1))
        missions = re.findall(r'"([^"]*)"', m.group(4))
        dishes = re.findall(r'"([^"]*)"', m.group(5))
        out.append(dict(index=i, id=m.group(2), name=m.group(3), continent=cont_ids[i // 5],
                        continent_name=conts[i // 5], missions=missions, dishes=dishes))
    return out


def main():
    countries = read_catalog()
    assert len(countries) == 35, len(countries)
    os.makedirs(OUT, exist_ok=True)
    rows = []
    for c in countries:
        places = PLACES[c['id']]
        assert len(places) == 27, (c['id'], len(places))
        titles = c['missions'][:3] + places
        polar = c['continent'] == 'antarctica'
        for n in range(1, 31):
            rel = 'Assets/Project Data/Game/Resources/World/%s/%s/level_%d.png' % (c['continent'], c['id'], n)
            exists = os.path.exists(os.path.join(PROJECT, rel))
            if exists:
                status = 'EXISTS'
            elif c['id'] == 'india' and n in INDIA_EXISTING:
                status = 'COPY from Images/LevelSelection/' + INDIA_EXISTING[n]
            else:
                status = 'GENERATE'
            dishes = ', '.join(c['dishes'][(n - 1) % 7:(n - 1) % 7 + 3] or c['dishes'][:3])
            prompt = (POLAR if polar else STYLE).format(place=titles[n - 1], country=c['name'], dishes=dishes)
            rows.append(dict(level_in_game=c['index'] * 30 + n, continent=c['continent_name'], country=c['name'],
                             level=n, title=titles[n - 1], file=rel, status=status, prompt=prompt))

    with open(os.path.join(OUT, 'card_art_list.csv'), 'w', newline='', encoding='utf-8-sig') as f:
        w = csv.DictWriter(f, fieldnames=list(rows[0].keys()))
        w.writeheader()
        w.writerows(rows)

    gen = [r for r in rows if r['status'] == 'GENERATE']
    have = [r for r in rows if r['status'] == 'EXISTS']
    copy = [r for r in rows if r['status'].startswith('COPY')]
    md = ['# Level Selection card pictures - complete list', '',
          '30 cards per country, one picture per level. **%d to generate**, %d already in the project, '
          '%d to copy (India).' % (len(gen), len(have), len(copy)), '',
          '## Specification', '',
          '* **Size:** 1024 x 768 px PNG (4:3, landscape), sRGB, no transparency needed',
          '* **File:** `Assets/Project Data/Game/Resources/World/<continent>/<country>/level_<N>.png` (N = 1-30)',
          '* **Style:** like the existing cards - painted landmark scene, food stall or table in front, '
          'golden-hour / twilight light, glowing lanterns, rich colours, no text',
          '* **Important area:** the card shows roughly the middle 85% - keep the landmark and the food away from the edges',
          '* **Title:** shown on the card under the picture (column "Title")', '',
          '## Count by continent', '', '| Continent | Countries | To generate |', '| --- | --- | --- |']
    for cont in dict.fromkeys(r['continent'] for r in rows):
        n_gen = sum(1 for r in gen if r['continent'] == cont)
        md.append('| %s | %s | %d |' % (cont, ', '.join(dict.fromkeys(r['country'] for r in rows if r['continent'] == cont)), n_gen))
    md += ['| **Total** | 35 | **%d** |' % len(gen), '']
    for c in countries:
        md += ['## %s - %s' % (c['continent_name'], c['name']), '', '| Level | Title | File | Status |', '| --- | --- | --- | --- |']
        for r in rows:
            if r['country'] == c['name']:
                md.append('| %d | %s | `level_%d.png` | %s |' % (r['level'], r['title'], r['level'], r['status']))
        md.append('')
    md += ['## Prompt template', '', '```', STYLE.format(place='<Title>', country='<Country>', dishes='<3 dishes>'), '```', '',
           'Antarctica:', '', '```', POLAR.format(place='<Title>', country='<Region>', dishes='<3 dishes>'), '```', '',
           'Every row of `card_art_list.csv` has its own ready-made prompt (column "prompt").']
    open(os.path.join(OUT, 'card_art_list.md'), 'w', encoding='utf-8').write('\n'.join(md) + '\n')

    # the card titles the game shows: line N = title of game level N (Resources/World/card_titles.txt)
    titles_path = os.path.join(WORLD, 'card_titles.txt')
    ordered = sorted(rows, key=lambda r: r['level_in_game'])
    open(titles_path, 'w', encoding='utf-8', newline='\n').write('\n'.join(r['title'] for r in ordered) + '\n')
    print('rows %d  generate %d  exists %d  copy %d  -> %s' % (len(rows), len(gen), len(have), len(copy), OUT))
    print('card titles -> %s' % os.path.relpath(titles_path, PROJECT))


if __name__ == '__main__':
    main()
