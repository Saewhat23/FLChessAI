using UnityEngine;

namespace Board
{
    /**
     * Graveyard
     *
     * Represents graveyard for each player and adjusts piece location and properties on death.
     */
    public class Graveyard : MonoBehaviour
    {
        private int _pieceCount;
        private int _rowCount;
        private Vector3 _position;
        private const float Offset = 2.5f;

        public void AddToGrave(Transform piece)
        {
            // Disable interaction
            piece.gameObject.GetComponent<BoxCollider2D>().enabled = false;
            piece.SetParent(transform);
        
            // Determine row and column
            if (_pieceCount % 4 == 0)
            {
                _rowCount++;
            }
            int colCount = _pieceCount % 4;
        
            // Set position
            _position = new Vector3(colCount * Offset, -1 * _rowCount * Offset, 10);
            piece.localPosition = _position;
            _pieceCount++;
        }
    }
}
