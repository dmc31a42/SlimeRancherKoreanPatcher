using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTextMeshComponent : MonoBehaviour {

    TMPro.TMP_Text m_TMP_Text;
	// Use this for initialization
	void Start () {
        m_TMP_Text = GetComponent<TMPro.TMP_Text>();

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
