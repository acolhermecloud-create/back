using Domain;
using Domain.Interfaces.Repository;
using Domain.Interfaces.Services;
using Util;

namespace Service
{
    public class FakeDataForCampaignService : IFakeDataForCampaignService
    {
        private static readonly List<string> Names = new()
        {
            "João Silva", "Maria Souza", "Pedro Santos", "Ana Oliveira", "Carlos Lima",
            "Fernanda Rocha", "Gabriel Mendes", "Juliana Alves", "Ricardo Martins", "Camila Ribeiro",
            "Bruno Correia", "Tatiane Teixeira", "Lucas Almeida", "Vanessa Carvalho", "Thiago Barbosa",
            "Mariana Ferreira", "Rafael Costa", "Isabela Nogueira", "Felipe Gomes", "Caroline Azevedo",
            "Anderson Monteiro", "Patricia Figueiredo", "Victor Duarte", "Larissa Lopes", "Rodrigo Batista",
            "Jessica Pereira", "Daniela Coutinho", "Alexandre Rezende", "Sabrina Monteiro", "Leonardo Cardoso",
            "Juliana Lima", "Fernando Souza", "Camila Martins", "Eduardo Silva", "Marta Costa",
            "Roberta Almeida", "Gustavo Pires", "Patrícia Lima", "Cláudio Ferreira", "Renata Barbosa",

            "André Moreira", "Letícia Dias", "Bruno Fonseca", "Tatiana Ramos", "Diego Lima",
            "Carla Martins", "Renan Azevedo", "Priscila Barbosa", "Luciano Rocha", "Alessandra Nunes",
            "Henrique Castro", "Daniel Moraes", "Natália Vieira", "Cássio Almeida", "Fabiana Teixeira",
            "Marcelo Gonçalves", "Beatriz Souza", "Vinícius Mendes", "Paula Fernandes", "Cristiano Silva",
            "Helena Duarte", "Robson Monteiro", "Débora Lima", "Fábio Rocha", "Suelen Nogueira",
            "Maurício Costa", "Eliane Barbosa", "Otávio Martins", "Valéria Santos", "Tiago Nascimento",
            "Tainá Cardoso", "Leandro Freitas", "Nathalia Cunha", "Igor Santana", "Kelly Oliveira",
            "Márcio Ribeiro", "Silvia Pereira", "Jonathan Lopes", "Elaine Castro", "Rodrigo Camargo",
            "Luciana Melo", "Murilo Barbosa", "Viviane Rocha", "Douglas Almeida", "Bruna Machado",
            "Cristina Soares", "Danilo Moreira", "Simone Reis", "Caio Oliveira", "Aline Monteiro",

            "Edson Carvalho", "Lorena Ferreira", "Hugo Martins", "Yasmin Souza", "Wagner Rocha",
            "Milena Cardoso", "Jean Oliveira", "Lívia Castro", "Alan Pires", "Melissa Gomes",
            "Samuel Duarte", "Andressa Lima", "Caio Martins", "Renata Souza", "Jonas Almeida",
            "Érica Nogueira", "Vitor Ramos", "Talita Mendes", "Nelson Teixeira", "Adriana Silva",
            "Ruan Barbosa", "Jéssica Moreira", "Mateus Gonçalves", "Vanessa Fernandes", "Jorge Reis",
            "Cíntia Machado", "Luan Ribeiro", "Tatiane Moura", "Bruno Costa", "Júlia Castro",
            "Ivan Rocha", "Cristiane Monteiro", "Hélio Almeida", "Patrícia Santos", "Diogo Camargo",
            "Gabriela Lopes", "Éverton Ferreira", "Marina Costa", "Caio Rezende", "Elaine Figueiredo",
            "Alex Lima", "Amanda Nogueira", "Eduarda Ribeiro", "Giovanni Oliveira", "Natasha Mendes",
            "Jeferson Souza", "Bárbara Martins", "Sandro Cardoso", "Lorraine Silva", "Túlio Correia"
        };

        private static readonly HashSet<string> UsedNames = new HashSet<string>();

        public static List<string> SupportMessages = new()
        {
            "Galera, juntos a gente consegue! Bora doar e ajudar quem tá precisando!",
            "Toda ajuda conta muito! Vamos espalhar solidariedade e mudar histórias reais!",
            "Você pode fazer toda diferença hoje. Bora ajudar e levar esperança pra alguém!",
            "Tô torcendo demais pra essa campanha dar certo! Vem com a gente nessa",
            "Bora doar de coração, pessoal! Um pequeno gesto muda muita coisa",
            "Quanto mais gente participar, mais esperança a gente leva. Vamos juntos nessa corrente do bem",
            "Cada contribuição é um passo importante! Não vamos desistir de ajudar.",
            "Mostrar amor ao próximo é o melhor que podemos fazer. Vamos fazer a diferença juntos",
            "Que tal ser uma luz na vida de alguém hoje? Sua doação pode mudar tudo.",
            "Não importa o valor, o que importa é participar! Bora ajudar nessa causa",
            "Doar é um ato lindo de amor. Vamos fazer isso juntos!",
            "Quem doa, realmente transforma vidas! Entra nessa corrente com a gente!",
            "Sua ajuda é essencial pra que mais pessoas sejam alcançadas. Conto com você!",
            "Cada atitude solidária leva esperança pra quem precisa de verdade.",
            "Ser gentil hoje é garantir felicidade amanhã. Bora fazer nossa parte!",
            "Uma atitude simples já faz muita diferença. Bora doar!",
            "Solidariedade é união. Sua doação pode salvar o dia de alguém!",
            "Generosidade não tem limites! Vamos espalhar esse amor!",
            "Doando, você leva esperança para quem precisa muito!",
            "Toda ajuda é bem-vinda mesmo! Bora fazer a diferença juntos. 💖",
            "Cada doação é um sorriso a mais. Vamos espalhar felicidade! ",
            "Doa de coração e sente como pequenos gestos mudam vidas!",
            "A mudança começa com você! Apoie e inspire outras pessoas!",
            "Se cada um fizer um pouquinho, juntos faremos algo incrível! 💪",
            "O amanhã fica melhor quando ajudamos uns aos outros!",
            "Não espera os outros fazerem não, bora ser essa mudança!",
            "Seu gesto pode salvar uma vida! Bora ajudar!",
            "A ajuda que você dá hoje vira esperança amanhã!",
            "Doar é simples, mas pode fazer um bem enorme. Vem com a gente!",
            "Doe com carinho e ajude a espalhar esperança!",
            "Quanto mais ajudamos, mais forte fica essa corrente do bem!",
            "Bora doar e motivar mais gente a fazer o mesmo. Juntos somos mais fortes!",
            "Cada doação faz o mundo melhor, uma pessoa de cada vez!",
            "Você tem o poder de fazer a diferença. Vamos espalhar solidariedade!",
            "Espalhar amor através da doação torna o mundo melhor!",
            "Mude a vida de alguém hoje! Doe e espalhe coisas boas!",
            "Cada contribuição é uma nova chance pra quem precisa!",
            "Um gesto gentil pode mudar completamente o dia de alguém!",
            "Sua solidariedade pode aquecer muitos corações. Vamos lá!",
            "Generosidade é um presente que nunca perde valor!",
            "Doar é plantar esperança pra um futuro melhor!",
            "Empatia muda vidas! Bora doar e ajudar quem precisa!",
            "Seja a mudança! Doe e leve esperança por aí!",
            "Juntos podemos criar um mundo melhor e mais justo!",
            "Qualquer contribuição já faz uma baita diferença! Bora ajudar!",
            "Não importa o valor, o que vale é o carinho que colocamos em cada doação!",
            "Doe e veja o bem se multiplicar rapidinho!",
            "Cada contribuição fortalece essa corrente solidária. Bora participar!",
            "Ajudar faz bem demais! Experimenta e sente essa alegria!",
            "Amar o próximo é agir com solidariedade. Doe com carinho!",
            "Seja o motivo do sorriso de alguém hoje. Faça uma doação!",
            "A esperança cresce com cada gesto generoso. Conto contigo!",
            "Sua doação pode ser a nova chance que alguém tá esperando!",
            "Momentos especiais acontecem com gestos simples. Vamos doar!",
            "Atitudes pequenas geram grandes mudanças. Bora ajudar!",
            "A solidariedade é essencial pra um mundo melhor. Vamos nessa missão juntos!",
            "Doe e leve esperança para quem precisa",
            "Cada doação abre novas portas pra quem precisa muito!",
            "Transforme sua compaixão em ação! Faça a diferença agora!",
            "Sua generosidade pode realmente mudar a vida de alguém!",
            "Vamos construir um amanhã melhor com atitudes de amor e solidariedade!",
            "Cada doação espalha amor pelo mundo. Vem fazer parte disso! ",
            "O mundo precisa de mais atitudes legais. Começa agora! 😊",
            "Uma atitude sua pode ser um presente gigante pra alguém! 🎁",
            "Doar é levar esperança. Bora mudar histórias com a gente!",
            "Seu apoio transforma vidas e ajuda sonhos a virarem realidade!",
            "Solidariedade faz a gente ser mais humano. Bora ajudar! 🤝",
            "Quem doa com amor espalha felicidade!",
            "Seja motivo de esperança pra alguém hoje! Faça sua doação!",
            "Sua ajuda mostra o quanto você é especial. Continua assim",
            "Cada contribuição ilumina a vida de alguém. Seja essa luz!",
            "Ajudando uns aos outros, criamos paz e amor no mundo.",
            "Juntos podemos fazer coisas incríveis. Bora doar",
            "Você pode ser a chave pra mudar a vida de alguém.",
            "Com cada atitude positiva, fazemos um mundo melhor. Bora nessa missão! 🌍",
            "Sua ajuda hoje traz esperança pra muita gente amanhã! 🙌",
            "Doar é compartilhar amor, e disso o mundo sempre precisa! 💖"
        };

        private static readonly Random Random = new();

        private static string GenerateRandomCPF()
        {
            int[] numbers = new int[11];
            for (int i = 0; i < 9; i++)
            {
                numbers[i] = Random.Next(0, 9);
            }
            numbers[9] = CalculateCpfDigit(numbers, 9);
            numbers[10] = CalculateCpfDigit(numbers, 10);
            return string.Join("", numbers);
        }

        private static int CalculateCpfDigit(int[] numbers, int length)
        {
            int sum = 0, weight = length + 1;
            for (int i = 0; i < length; i++)
            {
                sum += numbers[i] * weight--;
            }
            int remainder = sum % 11;
            return remainder < 2 ? 0 : 11 - remainder;
        }

        private static string GenerateRandomEmail(string name)
        {
            string normalized = name.ToLower().Replace(" ", "");
            string domain = "gmail.com";
            return $"{normalized}{Random.Next(100, 999)}@{domain}";
        }

        private readonly IUserRepository _userRepository;
        private readonly IUtilityService _utilityService;
        private readonly ICampaignRepository _campaignRepository;
        private readonly ICampaignDonationRepository _campaignDonationRepository;
        private readonly ICampaignCommentsRepository _commentsRepository;

        public FakeDataForCampaignService(
            IUserRepository userRepository,
            IUtilityService utilityService,
            ICampaignCommentsRepository commentsRepository,
            ICampaignDonationRepository campaignDonationRepository,
            ICampaignRepository campaignRepository)
        {
            _userRepository = userRepository;
            _utilityService = utilityService;

            _commentsRepository = commentsRepository;
            _campaignDonationRepository = campaignDonationRepository;
            _campaignRepository = campaignRepository;
        }

        private async Task<User> CreateUser()
        {
            string randomName = GenerateUniqueName();
            string randomEmail = GenerateRandomEmail(randomName);
            string randomCPF = GenerateRandomCPF();

            User user = new()
            {
                Mock = true,
                Name = randomName,
                DocumentId = randomCPF,
                Email = randomEmail,
                Password = _utilityService.CryptSHA256(Functions.GenerateTimeStampStrUnique()),
                Provider = AuthProvider.None,
                Type = UserType.Common
            };

            return user;
        }

        public async Task CreateForCampaign(string slug, bool allowDonations, long? goal)
        {
            var campaignRepo = await _campaignRepository.GetBySlug(slug);
            campaignRepo.CanReceiveDonation = allowDonations;

            int minRange = 2542;
            int maxRange = 38281;

            Random random = new();
            int totalDoado = 0;
            int comentarioCount = 0;
            int doacaoCount = 0;

            while ((goal == null && doacaoCount < random.Next(8, 36)) || (goal != null && totalDoado < goal))
            {
                var user = await CreateUser();
                int valorDoacao = random.Next(minRange, maxRange + 1);

                DateTime startDate = campaignRepo.CreatedAt;
                DateTime endDate = DateTime.Now;
                TimeSpan timeSpan = endDate - startDate;

                if (timeSpan.TotalMinutes <= 60)
                    continue; // campanha criada há menos de 1h

                await _userRepository.Add(user);

                DateTime randomDate = startDate.AddMinutes(random.Next(0, (int)timeSpan.TotalMinutes));

                Donation donation = new(
                    campaignRepo.Id,
                    user.Id,
                    Guid.NewGuid().ToString(),
                    DonationType.Money,
                    TransationMethod.Cash,
                    valorDoacao,
                    1,
                    randomDate,
                    DonationStatus.Paid,
                    DonationBalanceStatus.WaitingForRelease,
                    Gateway.Internal);

                await _campaignDonationRepository.Add(donation);
                doacaoCount++;
                totalDoado += valorDoacao;

                // Cria comentário apenas se ainda não passou 40% do total de doações
                if (comentarioCount < doacaoCount * 0.4)
                {
                    string randomMessage = SupportMessages[random.Next(SupportMessages.Count)];
                    randomDate = startDate.AddMinutes(random.Next(0, (int)timeSpan.TotalMinutes));

                    CampaignComments comment = new(campaignRepo.Id, user.Id, randomMessage, randomDate);
                    await _commentsRepository.Add(comment);
                    comentarioCount++;
                }

                await _campaignRepository.Update(campaignRepo);
            }
        }
        private string GenerateUniqueName()
        {
            string randomName;
            do
            {
                string firstName1 = Names[Random.Next(Names.Count)].Split(' ')[0];
                string firstName2 = Names[Random.Next(Names.Count)].Split(' ')[0];
                string lastName = Names[Random.Next(Names.Count)].Split(' ')[1];

                // Evita nome composto repetido tipo "João João"
                while (firstName1 == firstName2)
                    firstName2 = Names[Random.Next(Names.Count)].Split(' ')[0];

                randomName = $"{firstName1} {firstName2} {lastName}";
            }
            while (UsedNames.Contains(randomName));

            UsedNames.Add(randomName);
            return randomName;
        }
    }
}
