using System.Linq;

namespace Lingua.Vocabulary
{
    public class Specification
    {
        public Specification(string[] variations, string? incompleteCompound, string modifiers)
        {
            Variations = variations;
            IncompleteCompound = incompleteCompound;
            Modifiers = modifiers;
        }

        public string[] Variations { get; }
        public string? IncompleteCompound { get; }
        public string Modifiers { get; }
        public string Base => Variations.First();

        public Specification[] Synonyms { get; set; }

        public string GetVariation(int index)
            => Variations[index % Variations.Length];
    }
}