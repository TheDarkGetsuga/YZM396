using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/SwordData")]
public class SwordData : ScriptableObject
{
    public enum WeaponType { Sword, Spear }

    public string swordName;
    public int level = 1;
    public float baseDamage = 10f;
    public float baseSwingSpeed = 0.3f;

    public Sprite swordSprite;
    public AudioClip[] swingSounds;
    public WeaponType weaponType = WeaponType.Sword;

    [Tooltip("Prefix used in Animator triggers and states. Example: Sword or Spear")]
    public string animationPrefix = "Sword";

    [Tooltip("How many hits are in this weapon's combo chain")]
    [Range(1, 10)]
    public int comboLength = 5;

    public float Damage => baseDamage * level;
    public float SwingSpeed => baseSwingSpeed / level;
}
