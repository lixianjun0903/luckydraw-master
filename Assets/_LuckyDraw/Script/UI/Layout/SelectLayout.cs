using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SelectLayout : MonoBehaviour
{
    private ToggleGroup toggleGroup;

    private void Start()
    {
        toggleGroup = gameObject.GetComponent<ToggleGroup>();
    }

    public int GetSelectedToggle()
    {
        Toggle selectedToggle = toggleGroup.ActiveToggles().FirstOrDefault();
        if (selectedToggle != null)
        {
            if (int.TryParse(selectedToggle.name, out int index))
            {
                return index;
            }
        }
        return -1;
    }

}
