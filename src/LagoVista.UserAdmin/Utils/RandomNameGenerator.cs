using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Utils
{
    public static class RandomNameGenerator
    {
        // Keep lists moderate in size. Even 256 adjectives x 256 nouns is plenty.
        // 65,536 combos * (32^4 suffix = 1,048,576) => huge space.
        private static readonly string[] _adjectives =
        {
            "able","active","adapted","adept","agile","alert","aligned","alive","ample","analytic",
            "anchored","animated","apt","arcane","ardent","artful","ascent","atomic","august","austere",
            "avid","awake","balanced","bamboo","basic","beaming","beloved","bent","beta","better",
            "big","binary","blazing","blessed","bold","bonny","boundless","brave","bright","brisk",
            "broad","buoyant","busy","calm","candid","capable","careful","casual","central","certain",
            "charmed","cheerful","chief","chosen","civic","clean","clear","clever","close","coastal",
            "cobalt","coherent","colossal","comic","compact","complete","composed","concrete","confident","constant",
            "cool","cosmic","courteous","crafty","creative","crisp","crystal","curious","daily","dapper",
            "daring","decent","deep","delightful","dense","direct","disciplined","distinct","divine","dormant",
            "driven","dynamic","eager","early","earthy","easy","electric","elegant","elite","ember",
            "eminent","enabled","endless","energetic","engaged","enriched","epic","equal","etched","even",
            "exact","expert","fabled","factual","fair","faithful","famous","fast","faultless","fearless",
            "fellow","fertile","fine","finite","firm","first","fit","fixed","fleet","flowing",
            "fluent","focused","fond","formal","frank","free","fresh","friendly","full","gentle",
            "genuine","giant","glad","global","golden","good","graceful","grand","graphic","great",
            "green","grounded","growing","guarded","guided","happy","hardy","harmonic","harvest","healthy",
            "hearty","helpful","hidden","high","hollow","honest","honey","hopeful","humble","ideal",
            "immense","immortal","improved","infinite","informed","inner","instant","intact","intense","intent",
            "inventive","iron","jade","jolly","joyful","just","keen","kind","kinetic","knowing",
            "labeled","lasting","leading","legendary","light","linear","lively","local","logical","lucky",
            "lunar","lush","major","making","mature","maximum","measured","mellow","mighty","minimal",
            "mint","modern","modest","moral","motion","moving","musical","muted","native","neat",
            "neutral","new","nifty","nimble","noble","noisy","normal","noted","novel","nuanced",
            "oaken","observant","oceanic","open","optimal","orderly","organic","original","outer","patient",
            "peaceful","pearl","perfect","persistent","pixel","plains","pleasant","polished","popular","portable",
            "positive","precise","prime","proud","pure","quick","quiet","radiant","rapid","rare",
            "ready","real","refined","regular","reliable","resolute","rich","right","rising","robust",
            "rooted","royal","rural","safe","sage","salt","sane","secure","select","serene",
            "sharp","shining","short","silent","silver","simple","sincere","single","skilled","smart",
            "smooth","snug","social","solid","sound","spare","special","spiral","stable","steady",
            "stellar","still","stone","stout","straight","strong","subtle","sunlit","sunny","super",
            "supreme","swift","tactful","tame","tangible","tasty","teal","tender","terrific","tidy",
            "timely","tiny","top","tranquil","true","trusted","tuned","twin","ultimate","unified",
            "unique","united","upbeat","upright","urban","usable","valid","vast","velvet","verbal",
            "vibrant","victorious","vital","vivid","warm","wavy","well","wholesome","wide","wild",
            "willing","wise","witty","wonderful","wooden","worthy","young","zen"
        };

        private static readonly string[] _nouns =
        {
            "acorn","anchor","angle","apple","arch","arena","arrow","atlas","aurora","badge",
            "bamboo","banner","barrel","basin","bay","beacon","beetle","bell","berry","biscuit",
            "blossom","bluebird","board","boat","boulder","branch","bridge","brook","bubble","bucket",
            "cabinet","cable","cactus","camera","camp","canyon","caravan","carbon","cardinal","cascade",
            "castle","cedar","cellar","center","chamber","channel","chapter","charm","cherry","chess",
            "circle","citadel","city","cliff","cloud","cluster","coast","comet","compass","concept",
            "coral","corner","cottage","cove","crane","crater","creek","crystal","cube","current",
            "dawn","delta","den","diamond","dome","doorway","drift","dune","echo","ember",
            "engine","entry","event","falcon","family","ferry","field","fjord","flame","flower",
            "fog","forest","forge","fountain","fox","frame","garden","gate","gecko","gem",
            "glade","glider","glow","granite","grove","harbor","harmony","haven","heart","hedge",
            "helium","hill","horizon","hull","iceberg","icon","island","jade","jigsaw","journey",
            "juniper","key","kingdom","kite","knoll","lagoon","lantern","leaf","legend","library",
            "lighthouse","lily","limit","linen","locket","logic","lotus","maple","marble","market",
            "meadow","mercury","mesa","meteor","mirror","mist","module","monolith","moon","mosaic",
            "mountain","museum","nebula","nest","net","node","notebook","oak","oasis","object",
            "ocean","olive","orbit","orchid","origin","otter","owl","paper","path","pebble",
            "peak","pearl","phoenix","piano","pine","pixel","planet","plate","plaza","pond",
            "prairie","project","pyramid","quartz","quest","radio","rain","raven","reef","river",
            "road","rocket","root","rose","sail","salt","sanctuary","sapphire","scale","scene",
            "signal","silver","sky","slate","snow","socket","song","spark","sphere","spice",
            "spring","stone","stream","summit","sun","system","tablet","talon","tangerine","temple",
            "thread","thunder","tide","timber","token","tower","trail","tree","triangle","tundra",
            "tunnel","valley","vector","vessel","violet","vista","wave","wharf","willow","wind",
            "window","wing","winter","wood","workshop","world","zenith"
        };

        // Crockford-ish base32 without ambiguous chars (I,L,O,U).
        private const string Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

        public static string Generate()
        {
            var adjective = Pick(_adjectives);
            var noun = Pick(_nouns);
            var suffix = RandomSuffix(4);

            return $"{adjective}-{noun}-{suffix}".ToLowerInvariant();
        }

        private static string Pick(string[] list)
        {
            var idx = RandomNumberGenerator.GetInt32(list.Length);
            return list[idx];
        }

        private static string RandomSuffix(int length)
        {
            Span<byte> bytes = stackalloc byte[length];
            RandomNumberGenerator.Fill(bytes);

            var sb = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                sb.Append(Alphabet[bytes[i] % Alphabet.Length]);
            }

            return sb.ToString();
        }
    }
}
