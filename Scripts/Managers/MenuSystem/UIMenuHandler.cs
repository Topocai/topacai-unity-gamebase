using UnityEngine;

namespace Topacai.Utils.MenuSystem
{
    public class UIMenuHandler : MonoBehaviour
    {
        [SerializeField] protected UIMenu _menu = new();
        public virtual UIMenu MainMenu
        {
            get
            {
                if ( _menu == null )
                {
                    _menu = new();
                }
                return _menu;
            }
        }
    }
}
