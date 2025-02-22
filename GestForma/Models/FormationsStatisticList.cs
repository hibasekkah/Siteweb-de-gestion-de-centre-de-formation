namespace GestForma.Models
{
    public class FormationsStatisticList
    {
        public string formationName { get; set; }
        public int nbrInscriTotal { get; set; }
        public int nbrInscriTotalCerti { get; set; }
        public List<AgeGroupVM> ageGroups { get; set; }

      
    }
}
