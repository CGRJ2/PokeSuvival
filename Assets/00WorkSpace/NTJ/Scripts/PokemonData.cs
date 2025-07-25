using UnityEngine;

namespace NTJ
{
    [CreateAssetMenu(fileName = "PokemonData", menuName = "ScriptableObjects/PokemonData")]
    public class PokemonData : ScriptableObject
    {
        public string pokemonName;
        public Sprite[] levelSprites;

        public int baseHP;
        public int baseAtk;
        public int baseDef;
        public int baseSpA;
        public int baseSpD;
        public int baseSpe;
    }
}
