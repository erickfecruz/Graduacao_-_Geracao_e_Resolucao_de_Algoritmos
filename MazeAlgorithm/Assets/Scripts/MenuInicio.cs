using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;

public class MenuInicio : MonoBehaviour {

	public GameObject SelecionarMaze;

	public GameObject canvasParaDestruir;

	public Text dados1;
	public Text dados2;
	public Text dados3;

	public Button btn1;
	public Button btn2;
	public Button btn3;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		string d1 = dados1.text.ToString();
		string d2 = dados2.text.ToString();
		string d3 = dados3.text.ToString();
		if (d1 != "" && d2 != "" && d3 != "") {
			//int num = Int32.Parse (userID);
			btn1.interactable = true;
			btn2.interactable = true;
			btn3.interactable = true;
		} else {
			btn1.interactable = false;
			btn2.interactable = false;
			btn3.interactable = false;
		}
	}

	public void AtivaArvore(){
		string d1 = dados1.text.ToString();
		string d2 = dados2.text.ToString();
		string d3 = dados3.text.ToString();

		Variaveis.TamanhoX = Int32.Parse (d2);
		Variaveis.TamanhoY = Int32.Parse (d1);
		Variaveis.VelocidadeCriacao = float.Parse(d3);

		SelecionarMaze.GetComponent<ArvoreBinaria> ().enabled = true;
		Destroy (canvasParaDestruir);
	}

	public void AtivaCrescimento(){
		
		string d1 = dados1.text.ToString();
		string d2 = dados2.text.ToString();
		string d3 = dados3.text.ToString();

		Variaveis.TamanhoX = Int32.Parse (d2);
		Variaveis.TamanhoY = Int32.Parse (d1);
		Variaveis.VelocidadeCriacao = float.Parse (d3);
			
		SelecionarMaze.GetComponent<BacktrackingAlgorithm> ().enabled = true;
		Destroy (canvasParaDestruir);
	}

	public void AtivaEller(){

		string d1 = dados1.text.ToString();
		string d2 = dados2.text.ToString();
		string d3 = dados3.text.ToString();

		Variaveis.TamanhoX = Int32.Parse (d2);
		Variaveis.TamanhoY = Int32.Parse (d1);
		Variaveis.VelocidadeCriacao = float.Parse (d3);

		SelecionarMaze.GetComponent<EllerAlgorithm> ().enabled = true;
		Destroy (canvasParaDestruir);
	}
}
