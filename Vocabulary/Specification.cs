namespace Lingua.Vocabulary
{
    public class Specification
    {
        public Specification(string[] variations, string incompleteCompound, string modifiers)
        {
            Variations = variations;
            IncompleteCompound = incompleteCompound;
            Modifiers = modifiers;
        }

        public string[] Variations { get; }
        public string IncompleteCompound { get; }
        public string Modifiers { get; }
    }
}