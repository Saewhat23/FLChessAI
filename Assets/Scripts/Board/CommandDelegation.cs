using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Board
{
    /**
     * CommandDelegation
     *
     * Manages user delegation of one piece from the king group to a bishop.
     */
    public class CommandDelegation : MonoBehaviour
    {
        public static CommandDelegation Instance;
    
        // Buttons for delegation - pops up when an acceptable piece is selected (in king group)
        public Button red;
        public Button blue;

        private void Awake()
        {
            Instance = this;
        }

        // Transition piece to certain group, saving state information
        public void Delegate(string division)
        {
            Player current = Game.Controller.GetCurrentPlayer();
            if (current.Commanders[division].Moved || current.Selected == null) return;
            current.Selected.DelegateTo(division);
            current.Delegated.Add(current.Selected);
            current.DelegatedDivisions.Add(current.SelectedDivision);
            current.Selected = null;
            current.SelectedDivision = null;
            gameObject.SetActive(false);
        }

        // Disable one of the delegation buttons when a bishop is captured
        public void DisableButton(string division)
        {
            if (division == "L")
            {
                red.interactable = false;
            }
            else
            {
                blue.interactable = false;
            }
        }

        // Reset the delegated piece
        public void Reset()
        {
            Player current = Game.Controller.GetCurrentPlayer();
            for (int i = 0; i < current.Delegated.Count; i++)
            {
                ChessPiece piece = current.Delegated[i];
                string division = current.DelegatedDivisions[i];
                piece.DelegateTo(division);
            }

            current.Delegated = new List<ChessPiece>();
            current.DelegatedDivisions = new List<string>();
        }
    }
}
