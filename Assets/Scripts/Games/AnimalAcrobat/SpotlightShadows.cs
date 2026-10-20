using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Components
{
    public class PrefabShadow : MonoBehaviour
    {
        [Header("Offsets")]
        [SerializeField] private Vector2 _perspectiveStrength = new Vector2(0.15f, 0.0f); 
        [SerializeField] private Vector2 _flatOffset = new Vector2(0f, -0.2f);
        
        [Header("Other")]
        [SerializeField] private Color _shadowColor = Color.black;
        [SerializeField] private int _sortingOrderOffset = -555;
        [SerializeField] private List<SpriteRenderer> _ignoreList = new List<SpriteRenderer>();

        private List<ShadowPart> _shadowParts = new List<ShadowPart>();
        private GameObject _shadowRoot;
        private Transform _spotlightMain;

        private void Start()
        {
            if (_spotlightMain == null)
            {
                var gameScript = GetComponentInParent<Games.AnimalAcrobat>();
                if (gameScript != null)
                {
                    _spotlightMain = gameScript.transform.Find("SpotlightMain");
                    if (_spotlightMain == null)
                    {
                        var allTransforms = gameScript.GetComponentsInChildren<Transform>(true);
                        foreach (var t in allTransforms)
                        {
                            if (t.name == "SpotlightMain")
                            {
                                _spotlightMain = t;
                                break;
                            }
                        }
                    }
                }
            }

            if (_spotlightMain == null)
            {
                _spotlightMain = transform.parent; 
            }

            _shadowRoot = new GameObject($"{name}_ShadowRoot");
            _shadowRoot.transform.SetParent(transform.parent); 
            _shadowRoot.transform.localScale = Vector3.one;

            var allSprites = GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var originalSprite in allSprites)
            {
                if (_ignoreList.Contains(originalSprite)) continue;
                CreateShadowPart(originalSprite);
            }
        }

        private void CreateShadowPart(SpriteRenderer original)
        {
            GameObject partObj = new GameObject($"{original.name}_Shadow");
            partObj.transform.SetParent(_shadowRoot.transform);

            SpriteRenderer shadowRenderer = partObj.AddComponent<SpriteRenderer>();
            shadowRenderer.color = _shadowColor;
            shadowRenderer.sortingLayerID = original.sortingLayerID;
            shadowRenderer.sortingOrder = original.sortingOrder + _sortingOrderOffset;
            shadowRenderer.maskInteraction = original.maskInteraction;

            ShadowPart part = new ShadowPart
            {
                OffsetTransform = partObj.transform,
                ShadowRenderer = shadowRenderer,
                SourceRenderer = original
            };

            _shadowParts.Add(part);
        }

        private void LateUpdate()
        {
            if (_shadowRoot == null || _spotlightMain == null) return;

            bool spotlightActive = _spotlightMain.gameObject.activeInHierarchy;
            if (_shadowRoot.activeSelf != spotlightActive)
            {
                _shadowRoot.SetActive(spotlightActive);
            }
            if (!spotlightActive) return;

            Vector3 distFromCenter = transform.position - GameCamera.AdditionalPosition;
            Vector3 dynamicOffset = new Vector3(
                distFromCenter.x * _perspectiveStrength.x,
                distFromCenter.y * _perspectiveStrength.y,
                0
            );

            Vector3 finalShadowOffset = dynamicOffset + (Vector3)_flatOffset;

            for (int i = _shadowParts.Count - 1; i >= 0; i--)
            {
                var part = _shadowParts[i];

                if (part.SourceRenderer == null)
                {
                    Destroy(part.OffsetTransform.gameObject);
                    _shadowParts.RemoveAt(i);
                    continue;
                }

                part.ShadowRenderer.enabled = part.SourceRenderer.enabled && part.SourceRenderer.gameObject.activeInHierarchy;
                if (!part.ShadowRenderer.enabled) continue;

                part.ShadowRenderer.sprite = part.SourceRenderer.sprite;
                part.ShadowRenderer.flipX = part.SourceRenderer.flipX;
                part.ShadowRenderer.flipY = part.SourceRenderer.flipY;
                part.ShadowRenderer.sortingOrder = part.SourceRenderer.sortingOrder + _sortingOrderOffset;

                part.OffsetTransform.position = part.SourceRenderer.transform.position + finalShadowOffset;
                part.OffsetTransform.rotation = part.SourceRenderer.transform.rotation;
                part.OffsetTransform.localScale = part.SourceRenderer.transform.lossyScale;
            }
        }

        private void OnEnable()
        {
            if (_shadowRoot != null && _spotlightMain != null) 
            {
                _shadowRoot.SetActive(_spotlightMain.gameObject.activeInHierarchy);
            }
        }

        private void OnDisable()
        {
            if (_shadowRoot != null) _shadowRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_shadowRoot != null) Destroy(_shadowRoot);
        }

        private class ShadowPart
        {
            public Transform OffsetTransform;
            public SpriteRenderer ShadowRenderer;
            public SpriteRenderer SourceRenderer;
        }
    }
}