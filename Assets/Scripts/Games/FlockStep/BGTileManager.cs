using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_FlockStep
{
    public class BGTileManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Transform _bgTileFirst;
        [SerializeField] private Transform _bgTileSecond;

        private List<Transform> _tiles = new List<Transform>();
        private float _tileWidth;
        private int _poolSize = 4; //lowkey who cares

        private void Awake()
        {
            if (_bgTileFirst == null || _bgTileSecond == null) return;
            
            _tileWidth = Mathf.Abs(_bgTileSecond.localPosition.x - _bgTileFirst.localPosition.x);
            if (_tileWidth == 0) _tileWidth = 20f;
            _bgTileSecond.localPosition = new Vector3(_bgTileFirst.localPosition.x - _tileWidth, _bgTileSecond.localPosition.y, _bgTileSecond.localPosition.z);

            float startX = _bgTileFirst.localPosition.x;

            _tiles.Add(_bgTileFirst);
            _tiles.Add(_bgTileSecond);
            
            for (int i = 2; i < _poolSize; i++)
            {
                Transform template = (i % 2 == 0) ? _bgTileFirst : _bgTileSecond;
                var newTile = Instantiate(template, template.parent);
                newTile.localPosition = new Vector3(startX - (_tileWidth * i), template.localPosition.y, template.localPosition.z);
                _tiles.Add(newTile);
            }
        }

        private void LateUpdate()
        {
            float camX = GameCamera.instance.transform.position.x;
            
            foreach (var tile in _tiles)
            {
                if (tile.position.x > camX + 15f + (_tileWidth * 0.5f))
                {
                    tile.localPosition = new Vector3(tile.localPosition.x - (_tileWidth * _poolSize), tile.localPosition.y, tile.localPosition.z);
                }
            }
        }
    }
}
