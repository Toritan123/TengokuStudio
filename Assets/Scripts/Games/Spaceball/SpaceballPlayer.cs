using System;
using System.Collections.Generic;

using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_Spaceball
{
    public class SpaceballPlayer : MonoBehaviour
    {
        private Animator _anim;
        private int _currentCostume;
        private int _currentMask;

        public SpriteRenderer PlayerSprite;
        public SpriteRenderer Hat;
        public SpriteRenderer Bat;
        public Color[] BatColors;

        public HatSprite HatSprites1 = new HatSprite();
        public HatSprite HatSprites2 = new HatSprite();
        public HatSprite HatSprites4 = new HatSprite();
        public HatSprite HatSprites5 = new HatSprite();
        public HatSprite HatSprites3 = new HatSprite();

        [Serializable]
        public struct HatSprite
        {
            public List<Vector2> Offsets;
            public List<Sprite> Sprites;
        }

        public static SpaceballPlayer instance { get; set; }

        private void Awake()
        {
            instance = this;
            _anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(Spaceball.InputAction_BasicPress, out _))
            {
                Swing(null);
            }
        }

        public void SetCostume(Material mat, int costumeIndex, int maskIndex)
        {
            PlayerSprite.material = mat;
            _currentCostume = costumeIndex;
            _currentMask = costumeIndex == (int)Spaceball.CostumeType.Custom ? maskIndex : _currentCostume;
            _anim.Play("Idle", 0, 0);
            Bat.color = BatColors[costumeIndex];
        }

        public void Swing(SpaceballBall b)
        {
            if (b == null)
            {
                SoundByte.PlayOneShotGame("spaceball/swing");
            }
            else
            {

            }
            _anim.Play("Swing", 0, 0);
        }

        public void SetHatFrame(int frame)
        {
            // Unity can't serialize lists inside lists in this version, so that's annoying.
            var sprites = new HatSprite();
            switch (_currentMask)
            {
                case 1:
                    sprites = HatSprites1;
                    break;
                case 2:
                    sprites = HatSprites2;
                    break;
                case 4:
                    sprites = HatSprites4;
                    break;
                case 5:
                    sprites = HatSprites5;
                    break;
                default:
                    Hat.sprite = null;
                    return;
            }
            if (sprites.Sprites.Count - 1 < frame)
                frame = 0;
            Hat.sprite = sprites.Sprites[frame];

            var offset = Vector2.zero;
            if (sprites.Offsets.Count - 1 >= frame)
                offset = sprites.Offsets[frame];
            Hat.transform.localPosition = offset;
        }
    }
}