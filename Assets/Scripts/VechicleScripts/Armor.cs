using UnityEngine;
using UnityEngine.Events;

public class Armor : MonoBehaviour
{
    [Header("Typ pancerza")]
    [Tooltip("Wybierz typ pancerza z listy")]
    public ArmorType armorType = ArmorType.RHA;
    [Tooltip("grubo�� w milimetrach")]
    public float armorThickness = 10f; // Grubo�� pancerza w mm

    public float effectiveThickness; // Efektywna grubo�� pancerza

    private int armorTypeIndex; // Indeks typu pancerza (dla �atwego dost�pu)

    private void Start()
    {
        armorTypeIndex = (int)armorType; // Inicjalizacja indeksu typu pancerza
        effectiveThickness = armorThickness * armorTypeMultipliers[armorTypeIndex];
    }
    
 
    public enum ArmorType
    {
        RHA,            // Rolled Homogeneous Armor (standardowa stal)
        CastSteel,      // Stal odlewana
        Wood,           // Drewno
        Aluminium,      // Aluminium
    }

    // S�ownik z mno�nikami efektywnej grubo�ci dla r�nych typ�w pancerza
    private static readonly float[] armorTypeMultipliers = {
        1.0f,   // RHA (baza)
        0.9f,   // Stal odlewana (mniej efektywna ni� RHA)
        0.1f,   // Drewno (prawie bez ochrony)
        0.5f,   // Aluminium (�rednia ochrona)
    };

}
