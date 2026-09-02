using UnityEngine;

[CreateAssetMenu(menuName = "DestopDunge/Character Data", fileName = "NewCharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;
    public CharacterClass characterClass = CharacterClass.Monster;

    [Header("Tamagotchi (Desktop)")]
    [Tooltip("Prefab ระดับเล็กสำหรับโหมด Tamagotchi (2D หรือ 3D Low-poly)")]
    public GameObject desktopPrefab;
    public Sprite desktopIcon;
    [Tooltip("เวลาที่ใช้ในการฟักไข่ (วินาที)")]
    public float hatchTime = 60f;
    [Tooltip("เวลาที่ใช้ในการเติบโต/เปลี่ยนขั้น (วินาที)")]
    public float growthTime = 300f;

    [Header("Hack & Slash (Combat)")]
    [Tooltip("Prefab ร่างต่อสู้เต็มตัว (Full 3D)")]
    public GameObject combatPrefab;
    [Tooltip("Animator Controller สำหรับการโจมตี/คอมแบท")]
    public RuntimeAnimatorController combatAnimatorController;

    [Header("Base Stats")]
    public int maxHP = 100;
    public int attack = 10;
    [Tooltip("จำนวนครั้งโจมตีต่อวินาที")]
    public float attackSpeed = 1.0f;
}

public enum CharacterClass
{
    Monster,
    SexySpellblade,
    HeavyValkyrie
}
