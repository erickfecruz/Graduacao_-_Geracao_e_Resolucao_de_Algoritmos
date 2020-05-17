using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EllerAlgorithm : MonoBehaviour {
	[System.Serializable]
	public class Celula {
		public bool visitado = false;
		public GameObject norte;
		public GameObject sul;
		public GameObject leste;
		public GameObject oeste;
		public GameObject chao;
		public int valor;
		public int vizinhos;
		public int distancia;
		public bool visitadoAlgoritmo;
	}

	public InputField input01;
	public InputField input02;
	public InputField input03;
	public InputField input04;
	public InputField input05;
	public InputField input06;
	public InputField input07;
	public InputField input08;
	public InputField input09;
	public InputField input10;
	public InputField input11;
	public InputField input12;

	public Button infoBtn;
	public Button voltar;
	public Button seguidorParedes;
	public Button becosPreenche;
	public Button floodFill;
	public Button seguidor2;

	public GameObject btnVoltar;
	public GameObject btnInfo;
	public GameObject btnGera;
	public GameObject btnResol;
	public Transform menuFim;
	public Transform menu2;
	public Transform info;

	public GameObject algo1;
	public GameObject algo2;
	public GameObject algo3;
	public GameObject algo4;
	public GameObject escolherGerador;
	public GameObject parede;
	public GameObject piso;

	public Camera cameraPos;
	public float larguraParede = 1.0f;
	public int xSize = 5;
	public int ySize = 5;
	public float velCria = 0.1f;
	private Vector3 posInicial;
	private GameObject AgrupadorDeParedes;
	public Celula[] celulas;
	public int totalCel;
	private List<int> lastCells;
	private List<int> celAtiva;
	public int celAtual = 0;

	// Use this for initialization
	void Start () {

		Button b = btnInfo.GetComponent<Button> ();
		b.onClick.AddListener (() => dadosCorredores());

		Button c = btnVoltar.GetComponent<Button> ();
		c.onClick.AddListener (() => fechaMenuInfo());

		Button d = escolherGerador.GetComponent<Button> ();
		d.onClick.AddListener (() => restartGerador());	

		Button a1 = algo1.GetComponent<Button> ();
		a1.onClick.AddListener (() => Resolve01());

		Button a2 = algo2.GetComponent<Button> ();
		a2.onClick.AddListener (() => Resolve02());

		Button a3 = algo3.GetComponent<Button> ();
		a3.onClick.AddListener (() => Resolve03());

		Button a4 = algo4.GetComponent<Button> ();
		a4.onClick.AddListener (() => Resolve04());	

		Button b1 = btnGera.GetComponent<Button> ();
		b1.onClick.AddListener (() => restartGerador());	

		Button b2 = btnResol.GetComponent<Button> ();
		b2.onClick.AddListener (() => ResetaResolve());	

		xSize = Variaveis.TamanhoX;
		ySize = Variaveis.TamanhoY;
		velCria = Variaveis.VelocidadeCriacao;

		if (xSize >= ySize) {
			float tamanho = (float)xSize;
			if ((xSize % 2) == 0) {
				Vector3 camerPos = new Vector3 (0, tamanho, 0);
				cameraPos.orthographicSize = (tamanho / 2f) + 0.5f;
				cameraPos.transform.position += camerPos;
			} else {
				Vector3 camerPos = new Vector3 (0.5f, tamanho, 0.5f);
				cameraPos.orthographicSize = (tamanho / 2f) + 0.5f;
				cameraPos.transform.position += camerPos;
			}
		} else {
			float tamanho = (float)ySize;
			if ((ySize % 2) == 0) {
				Vector3 camerPos = new Vector3 (0, tamanho, 0);
				cameraPos.orthographicSize = (tamanho / 2f) + 0.5f;
				cameraPos.transform.position += camerPos;
			} else {
				Vector3 camerPos = new Vector3 (0.5f, tamanho, 0.5f);
				cameraPos.orthographicSize = (tamanho / 2f) + 0.5f;
				cameraPos.transform.position += camerPos;
			}
		}

		CreateWalls ();

	}

	//Função para geração do Grid com as paredes

	void CreateWalls(){
		AgrupadorDeParedes = new GameObject();
		AgrupadorDeParedes.name = "Labirinto";

		posInicial = new Vector3 ((-xSize / 2) + larguraParede / 2, 0.0f, (-ySize / 2) + larguraParede / 2);
		Vector3 myPos = posInicial;
		GameObject tempWall;
		GameObject tempPiso;

		//Eixo X

		for (int i = 0; i < ySize; i++) {
			for (int j = 0; j <= xSize; j++) {
				myPos = new Vector3 (posInicial.x + (j * larguraParede) - larguraParede / 2, 0.0f, posInicial.z + (i * larguraParede) - larguraParede / 2);
				tempWall = Instantiate (parede, myPos, Quaternion.identity) as GameObject;
				tempWall.transform.parent = AgrupadorDeParedes.transform;
			}
		}

		//Eixo Y

		for (int i = 0; i <= ySize; i++) {
			for (int j = 0; j < xSize; j++) {
				myPos = new Vector3 (posInicial.x + (j * larguraParede), 0.0f, posInicial.z + (i * larguraParede) - larguraParede);
				tempWall = Instantiate (parede,myPos, Quaternion.Euler(0.0f,90.0f,0.0f)) as GameObject;
				tempWall.transform.parent = AgrupadorDeParedes.transform;
			}
		}

		//Piso

		for (int i = 0; i < ySize; i++) {
			for (int j = 0; j < xSize; j++) {
				myPos = new Vector3 ((posInicial.x + (j * larguraParede) - larguraParede / 2) + 0.5f , -0.425f, posInicial.z + (i * larguraParede) - larguraParede / 2);
				tempPiso = Instantiate (piso, myPos, Quaternion.identity) as GameObject;
				tempPiso.transform.parent = AgrupadorDeParedes.transform;
			}
		}

		CreateCells();
	}

	//Função para Criar as Células
	void CreateCells(){
		totalCel = xSize * ySize;
		GameObject[] allWalls;
		int children = AgrupadorDeParedes.transform.childCount;
		allWalls = new GameObject[children];
		celulas = new Celula[xSize * ySize];
		int oesteLesteProcess = 0;
		int childProcess = 0;
		int termCount = 0;
		int chaoCount = (xSize * (ySize+1)) + (ySize * (xSize+1));

		//Pegar todos os Filhos
		for (int i = 0; i < children; i++) {
			allWalls [i] = AgrupadorDeParedes.transform.GetChild(i).gameObject;
		}

		//Atribuir Paredes as Células
		for(int cellProcess = 0; cellProcess < celulas.Length;cellProcess++){
			celulas[cellProcess] = new Celula();
			celulas[cellProcess].oeste = allWalls[oesteLesteProcess];
			celulas[cellProcess].sul = allWalls[childProcess + (xSize+1)*ySize];
			celulas[cellProcess].chao = allWalls[chaoCount];
			oesteLesteProcess++;
			chaoCount++;
			termCount++;
			childProcess++;
			celulas[cellProcess].visitadoAlgoritmo = false;
			celulas[cellProcess].distancia = totalCel + 1;
			celulas[cellProcess].leste = allWalls[oesteLesteProcess];
			celulas[cellProcess].norte = allWalls[(childProcess+(xSize+1)*ySize)+xSize-1];
			if (termCount == xSize){
				oesteLesteProcess++;
				termCount = 0;
			}
		}
		StartCoroutine(CreateMaze());
	}

	//Função para gerar o labirinto por meio do Algoritmo de Eller
	IEnumerator CreateMaze(){

		//Até a ultima linha
		for (int j = 0; j < ySize-1; j++) {

			//Colocar Valor em Todas as celulas sem valor
			for (int i = xSize*j; i < (xSize + (xSize*j)); i++) {
				if (celulas [i].valor == 0) {
					celulas [i].valor = i + 1;
				}
			}

			//Caso a celula nao seja do mesmo conjunto pode abrir uma parede ou nao
			for (int i = xSize*j; i < (xSize + (xSize*j)) - 1; i++) {
				if (celulas [i + 1].valor != celulas [i].valor) {
					if (Random.Range (0, 2) == 0) {
						int auxConjunto = celulas [i + 1].valor;
						for (int w = xSize * j; w < (xSize + (xSize * j)); w++) {
							if (celulas [w].valor == auxConjunto) celulas [w].valor = celulas [i].valor;
						}
						Destroy (celulas [i].leste);
					}
				}
			}


			//Pelo menos um valor de cada conjunto será passado para cima
			for (int i = xSize*j; i < (xSize + (xSize*j)); i++) {
				if (celulas [i].visitado == false) {
					List<int> mesmoConjunto = new List<int>();
					for (int w = i; w < (xSize + (xSize * j)); w++) {
						if (celulas [w].valor == celulas [i].valor) {
							mesmoConjunto.Add (w);
						}
					}

					while (mesmoConjunto.Count != 1){
						int m;
						m = Random.Range (0, mesmoConjunto.Count);
						if (Random.Range (0, 2) == 0) {
							Destroy (celulas[mesmoConjunto[m]].norte);
							celulas[mesmoConjunto[m] + xSize].valor = celulas[mesmoConjunto[m]].valor;
						}
						celulas[mesmoConjunto[m]].visitado = true;
						mesmoConjunto.RemoveAt (m);
					}
					Destroy (celulas[mesmoConjunto[0]].norte);
					celulas[mesmoConjunto[0]].visitado = true;
					celulas[mesmoConjunto[0] + xSize].valor = celulas[mesmoConjunto[0]].valor;
				}
			}

			yield return new WaitForSeconds (velCria);

		}

		//Última Linha

		//Colocar Valor em Todas as celulas sem valor
		for (int i = xSize* (ySize - 1); i < (xSize + (xSize*(ySize - 1))); i++) {
			if (celulas [i].valor == 0) {
				celulas [i].valor = i + 1;
			}
		}

		//Caso a celula nao seja do mesmo conjunto pode abrir uma parede ou nao
		for (int i = xSize*(ySize - 1); i < (xSize + (xSize*(ySize - 1))) - 1; i++) {
			if (celulas [i + 1].valor != celulas [i].valor) {
				int auxConjunto = celulas [i + 1].valor;
				for (int w = xSize *(ySize - 1); w < (xSize + (xSize * (ySize - 1))); w++) {
					if (celulas [w].valor == auxConjunto) celulas [w].valor = celulas [i].valor;
				}
				Destroy (celulas [i].leste);
			}
		}

		menu2.gameObject.SetActive (true);

	}


	public void dadosCorredores(){

		//Corredor Horizontal
		int maximoHorizontal = 0;
		int minimoHorizontal = totalCel;
		int corredorAtualHorizontal = 0;
		int numCorredoresHorizontal = 0;
		for (int i = 0; i < totalCel; i++) {
			if (celulas [i].leste == null) {
				corredorAtualHorizontal++;
			} else {
				corredorAtualHorizontal++;
				numCorredoresHorizontal++;
				if (corredorAtualHorizontal > maximoHorizontal)
					maximoHorizontal = corredorAtualHorizontal;
				if (corredorAtualHorizontal < minimoHorizontal)
					minimoHorizontal = corredorAtualHorizontal;
				corredorAtualHorizontal = 0;
			}
		}

		//Corredor Vertical
		int maximoVertical = 0;
		int minimoVertical = totalCel;
		int corredorAtualVertical = 0;
		int numCorredoresVertical = 0;
		int count1 = 0;
		int j = 0;
		while (count1 < totalCel) {
			Debug.Log (j);
			if (celulas [j].norte == null) {
				corredorAtualVertical++;
				j = j + xSize;
			} else {
				corredorAtualVertical++;
				numCorredoresVertical++;
				if (corredorAtualVertical > maximoVertical)
					maximoVertical = corredorAtualVertical;
				if (corredorAtualVertical < minimoVertical)
					minimoVertical = corredorAtualVertical;

				corredorAtualVertical = 0;

				if (j + xSize >= totalCel) {
					j = (j % xSize) + 1;
				} else
					j = j + xSize;
			}
			count1++;
		}

		float mediaCorredores = ((float)totalCel / (float)numCorredoresHorizontal);
		info.gameObject.SetActive (true);
		input01.text = ""+maximoHorizontal;
		input02.text = ""+minimoHorizontal;
		input03.text = ""+mediaCorredores;
		input04.text = ""+numCorredoresHorizontal;

		mediaCorredores = ((float)totalCel / (float)numCorredoresVertical);
		input06.text = ""+maximoVertical;
		input07.text = ""+minimoVertical;
		input08.text = ""+mediaCorredores;
		input09.text = ""+numCorredoresVertical;

		if (maximoHorizontal > maximoVertical) input10.text = ""+maximoHorizontal;
		else input10.text = ""+maximoVertical;

		if (minimoHorizontal < minimoVertical) input11.text = ""+minimoHorizontal;
		else input11.text = ""+minimoVertical;

		input12.text = ""+(numCorredoresVertical + numCorredoresHorizontal);

		//Becos
		int becos = 0;
		int count = 0;
		for (int i = 0; i < totalCel; i++) {
			if (celulas [i].leste == null)
				count++;
			if (celulas [i].oeste == null)
				count++;
			if (celulas [i].sul == null)
				count++;
			if (celulas [i].norte == null)
				count++;
			if (count == 1)
				becos++;
			count = 0;
		}
		input05.text = ""+becos;
		infoBtn.interactable = false;
		voltar.interactable = false;
		seguidorParedes.interactable = false;
		becosPreenche.interactable = false;
		floodFill.interactable = false;
		seguidor2.interactable = false;
	}

	public void fechaMenuInfo(){
		info.gameObject.SetActive (false);
		infoBtn.interactable = true;
		voltar.interactable = true;
		seguidorParedes.interactable = true;
		becosPreenche.interactable = true;
		floodFill.interactable = true;
		seguidor2.interactable = true;
	}

	public void restartGerador(){
		Application.LoadLevel (Application.loadedLevel);
	}

	public void Resolve01(){
		menu2.gameObject.SetActive (false);
		StartCoroutine(AlwaysLeft());
	}

	public void Resolve04(){
		menu2.gameObject.SetActive (false);
		StartCoroutine(AlwaysRight());
	}

	public void Resolve02(){
		menu2.gameObject.SetActive (false);
		StartCoroutine(DeadWay());
	}

	public void Resolve03(){
		menu2.gameObject.SetActive (false);
		StartCoroutine(FloodFill());
	}

	public void ResetaResolve(){
		menuFim.gameObject.SetActive (false);
		menu2.gameObject.SetActive (true);
		for (int celAtual = 0; celAtual < celulas.Length; celAtual++) {
			celulas [celAtual].chao.GetComponent<Renderer> ().material.color = Color.black;
			celulas [celAtual].vizinhos = 0;
			celulas [celAtual].visitadoAlgoritmo = false;
			celulas [celAtual].distancia = totalCel + 1;
		}
	}

	//----------------------------------------------------- ALGORITMOS DE RESOLUÇÃO -----------------------------------------------------//

	//Sempre a esquerda
	IEnumerator AlwaysLeft(){
		var temp = Time.realtimeSinceStartup;
		string frente = "leste";
		int percorre = xSize * (ySize - 1);
		celulas [percorre].chao.GetComponent<Renderer> ().material.color = Color.green;
		celulas [xSize-1].chao.GetComponent<Renderer> ().material.color = Color.red;
		while (percorre != xSize - 1) {
			if (frente == "leste") {
				if (celulas [percorre].norte == null) {
					percorre = percorre + xSize;
					frente = "norte";
				} else {
					if (celulas [percorre].leste == null) {
						percorre = percorre + 1;
					} else {
						frente = "oeste";
					}
				}
			} else if (frente == "norte") {
				if (celulas [percorre].oeste == null) {
					percorre = percorre - 1;
					frente = "oeste";
				} else {
					if (celulas [percorre].norte == null) {
						percorre = percorre + xSize;
					} else {
						frente = "sul";
					}
				}
			} else if (frente == "oeste") {
				if (celulas [percorre].sul == null) {
					percorre = percorre - xSize;
					frente = "sul";
				} else {
					if (celulas [percorre].oeste == null) {
						percorre = percorre - 1;
					} else {
						frente = "leste";
					}
				}
			} else if (frente == "sul") {
				if (celulas [percorre].leste == null) {
					percorre = percorre + 1;
					frente = "leste";
				} else {
					if (celulas [percorre].sul == null) {
						percorre = percorre - xSize;
					} else {
						frente = "norte";
					}
				}
			}
			yield return new WaitForSeconds (velCria);
			celulas [percorre].chao.GetComponent<Renderer> ().material.color = Color.green;
		}
		menuFim.gameObject.SetActive (true);
		var temp2 = Time.realtimeSinceStartup;
		Debug.Log ("Tempo de execução " + (temp2 - temp) + " segundos");
	}


	//Sempre a direita
	IEnumerator AlwaysRight(){
		var temp = Time.realtimeSinceStartup;
		string frente = "sul";
		int percorre = xSize * (ySize - 1);
		celulas [percorre].chao.GetComponent<Renderer> ().material.color = Color.green;
		celulas [xSize-1].chao.GetComponent<Renderer> ().material.color = Color.red;
		while (percorre != xSize - 1) {
			if (frente == "leste") {
				if (celulas [percorre].sul == null) {
					percorre = percorre - xSize;
					frente = "sul";
				} else {
					if (celulas [percorre].leste == null) {
						percorre = percorre + 1;
					} else {
						frente = "oeste";
					}
				}
			} else if (frente == "norte") {
				if (celulas [percorre].leste == null) {
					percorre = percorre + 1;
					frente = "leste";
				} else {
					if (celulas [percorre].norte == null) {
						percorre = percorre + xSize;
					} else {
						frente = "sul";
					}
				}
			} else if (frente == "oeste") {
				if (celulas [percorre].norte == null) {
					percorre = percorre + xSize;
					frente = "norte";
				} else {
					if (celulas [percorre].oeste == null) {
						percorre = percorre - 1;
					} else {
						frente = "leste";
					}
				}
			} else if (frente == "sul") {
				if (celulas [percorre].oeste == null) {
					percorre = percorre - 1;
					frente = "oeste";
				} else {
					if (celulas [percorre].sul == null) {
						percorre = percorre - xSize;
					} else {
						frente = "norte";
					}
				}
			}
			yield return new WaitForSeconds (velCria);
			celulas [percorre].chao.GetComponent<Renderer> ().material.color = Color.green;
		}
		menuFim.gameObject.SetActive (true);
		var temp2 = Time.realtimeSinceStartup;
		Debug.Log ("Tempo de execução " + (temp2 - temp) + " segundos");
	}



	//DeadWay Filling
	IEnumerator DeadWay(){
		var temp = Time.realtimeSinceStartup;
		celulas [xSize * (ySize - 1)].chao.GetComponent<Renderer> ().material.color = Color.green;
		celulas [xSize-1].chao.GetComponent<Renderer> ().material.color = Color.red;
		for(int celAtual = 0; celAtual < celulas.Length;celAtual++){
			celulas [celAtual].vizinhos = 0;
			if (celulas [celAtual].leste == null) celulas [celAtual].vizinhos++;
			if (celulas [celAtual].oeste == null) celulas [celAtual].vizinhos++;
			if (celulas [celAtual].sul == null) celulas [celAtual].vizinhos++;
			if (celulas [celAtual].norte == null) celulas [celAtual].vizinhos++;
		}

		celulas [xSize * (ySize - 1)].vizinhos = 0;
		celulas [xSize-1].vizinhos = 0;

		bool loopAtivo = true;
		while (loopAtivo) {
			loopAtivo = false;
			for(int i = 0; i < celulas.Length;i++){

				int check = 0;
				int aux = i;

				while (celulas [aux].vizinhos == 1) {
					celulas [aux].chao.GetComponent<Renderer> ().material.color = Color.blue;
					loopAtivo = true;

					celulas [aux].vizinhos = 0;
					//Não tem Célula ao Oeste

					check = ((aux + 1) / xSize);
					check -= 1;
					check *= xSize;
					check += xSize;

					bool encontrou = false;

					//Leste
					if (aux + 1 < totalCel && (aux + 1) != check && encontrou == false && celulas[aux].leste == null) {

						if (celulas [aux + 1].vizinhos != 0) {
							aux = aux + 1;
							celulas [aux].vizinhos--;
							encontrou = true;
						}

					} 

					//Oeste
					if ((aux - 1 >= 0) && (aux != check) && encontrou == false && celulas[aux].oeste == null) {

						if (celulas [aux - 1].vizinhos != 0) {
							aux = aux - 1;
							celulas [aux].vizinhos--;
							encontrou = true;
						}

					} 

					//Norte
					if (aux + xSize < totalCel && encontrou == false && celulas[aux].norte == null) {

						if (celulas [aux + xSize].vizinhos != 0) {
							aux = aux + xSize;
							celulas [aux].vizinhos--;
							encontrou = true;
						}

					}

					//Sul
					if (aux - xSize >= 0 && encontrou == false && celulas[aux].sul == null){

						if (celulas [aux - xSize].vizinhos != 0) {
							aux = aux -xSize;
							celulas [aux].vizinhos--;
							encontrou = true;
						}

					}

					yield return new WaitForSeconds (velCria);
				}

			}
		} 

		int aux2 = xSize * (ySize - 1);
		loopAtivo = true;
		while (loopAtivo) {
			bool encontrou = false;
			int check = ((aux2 + 1) / xSize);
			check -= 1;
			check *= xSize;
			check += xSize;
			//Leste
			if (aux2 + 1 < totalCel && (aux2 + 1) != check) {
				if ((celulas [aux2 + 1].chao.GetComponent<Renderer> ().material.color == Color.black || celulas [aux2 + 1].chao.GetComponent<Renderer> ().material.color == Color.red) && (encontrou == false) && (celulas [aux2].leste == null)) {
					aux2 = aux2 + 1;
					celulas [aux2].chao.GetComponent<Renderer> ().material.color = Color.green;
					encontrou = true;
				}
			}

			//Oeste
			if (aux2 - 1 >= 0 && aux2 != check) {
				if ((celulas [aux2 - 1].chao.GetComponent<Renderer> ().material.color == Color.black || celulas [aux2 - 1].chao.GetComponent<Renderer> ().material.color == Color.red) && (encontrou == false) && (celulas [aux2].oeste == null)) {
					aux2 = aux2 - 1;
					celulas [aux2].chao.GetComponent<Renderer> ().material.color = Color.green;
					encontrou = true;
				}
			}

			//Norte
			if (aux2 + xSize < totalCel) {
				if ((celulas [aux2 + xSize].chao.GetComponent<Renderer> ().material.color == Color.black || celulas [aux2 + xSize].chao.GetComponent<Renderer> ().material.color == Color.red) && (encontrou == false) && (celulas [aux2].norte == null)) {
					aux2 = aux2 + xSize;
					celulas [aux2].chao.GetComponent<Renderer> ().material.color = Color.green;
					encontrou = true;
				}
			}

			//Sul
			if (aux2 - xSize >= 0) {
				if ((celulas [aux2 - xSize].chao.GetComponent<Renderer> ().material.color == Color.black || celulas [aux2 - xSize].chao.GetComponent<Renderer> ().material.color == Color.red) && (encontrou == false) && (celulas [aux2].sul == null)) {
					aux2 = aux2 - xSize;
					celulas [aux2].chao.GetComponent<Renderer> ().material.color = Color.green;
					encontrou = true;
				}
			}
			yield return new WaitForSeconds (velCria);

			if (aux2 == xSize - 1) {
				loopAtivo = false;
			}
		}
		menuFim.gameObject.SetActive (true);
		var temp2 = Time.realtimeSinceStartup;
		Debug.Log ("Tempo de execução " + (temp2 - temp) + " segundos");
	}



	//FloodFill
	IEnumerator FloodFill(){
		var temp = Time.realtimeSinceStartup;

		//Inicializando
		int inicio = xSize * (ySize - 1);
		int fim = xSize - 1;
		celulas [inicio].distancia = 0;
		celulas [inicio].visitadoAlgoritmo = true;
		List<int> sequencia = new List<int>();
		sequencia.Add (inicio);

		//Pinta Cel inicio e fim
		celulas [inicio].chao.GetComponent<Renderer> ().material.color = Color.blue;
		celulas [fim].chao.GetComponent<Renderer> ().material.color = Color.red;

		//Loop até achar o fim
		while (!sequencia.Contains (fim)) {

			int aux = sequencia[0];

			int check = ((aux + 1) / xSize);
			check -= 1;
			check *= xSize;
			check += xSize;

			//Leste
			if (aux + 1 < totalCel && (aux + 1) != check) {
				if (celulas[aux].leste == null && celulas[aux + 1].visitadoAlgoritmo == false) {
					celulas [aux + 1].distancia = celulas [aux].distancia + 1;
					sequencia.Add (aux + 1);
					celulas [aux + 1].visitadoAlgoritmo = true;
					celulas [aux + 1].chao.GetComponent<Renderer> ().material.color = Color.blue;
				}
			}

			//Oeste
			if (aux - 1 >= 0 && aux != check) {
				if (celulas[aux].oeste == null && celulas[aux - 1].visitadoAlgoritmo == false) {
					celulas [aux - 1].distancia = celulas [aux].distancia + 1;
					sequencia.Add (aux - 1);
					celulas [aux - 1].visitadoAlgoritmo = true;
					celulas [aux - 1].chao.GetComponent<Renderer> ().material.color = Color.blue;
				}
			}

			//Norte
			if (aux + xSize < totalCel) {
				if (celulas[aux].norte == null && celulas[aux + xSize].visitadoAlgoritmo == false) {
					celulas [aux + xSize].distancia = celulas [aux].distancia + 1;
					sequencia.Add (aux + xSize);
					celulas [aux + xSize].visitadoAlgoritmo = true;
					celulas [aux + xSize].chao.GetComponent<Renderer> ().material.color = Color.blue;
				}
			}

			//Sul
			if (aux - xSize >= 0) {
				if (celulas[aux].sul == null && celulas[aux - xSize].visitadoAlgoritmo == false) {
					celulas [aux - xSize].distancia = celulas [aux].distancia + 1;
					sequencia.Add (aux - xSize);
					celulas [aux - xSize].visitadoAlgoritmo = true;
					celulas [aux - xSize].chao.GetComponent<Renderer> ().material.color = Color.blue;
				}
			}
			yield return new WaitForSeconds (velCria);
			sequencia.RemoveAt (0);

		}

		//Pinta Menor Caminho
		int valorFim = celulas [fim].distancia;
		int aux2 = fim;
		celulas [fim].chao.GetComponent<Renderer> ().material.color = Color.green;

		while (valorFim > 0) {

			bool encontrou = false;

			int check = ((aux2 + 1) / xSize);
			check -= 1;
			check *= xSize;
			check += xSize;

			//Leste
			if (aux2 + 1 < totalCel && (aux2 + 1) != check) {
				if (celulas[aux2].leste == null && (celulas[aux2 + 1].distancia == valorFim - 1) && (encontrou == false)) {
					aux2 = aux2 + 1;
					encontrou = true;
					valorFim--;
					celulas [aux2].chao.GetComponent<Renderer> ().material.color = Color.green;
				}
			}

			//Oeste
			if (aux2 - 1 >= 0 && aux2 != check) {
				if (celulas[aux2].oeste == null && (celulas[aux2 - 1].distancia == valorFim - 1) && (encontrou == false)) {
					aux2 = aux2 - 1;
					encontrou = true;
					valorFim--;
					celulas [aux2].chao.GetComponent<Renderer> ().material.color = Color.green;
				}
			}

			//Norte
			if (aux2 + xSize < totalCel) {
				if (celulas[aux2].norte == null && (celulas[aux2 + xSize].distancia == valorFim - 1) && (encontrou == false)) {
					aux2 = aux2 + xSize;
					encontrou = true;
					valorFim--;
					celulas [aux2].chao.GetComponent<Renderer> ().material.color = Color.green;
				}
			}

			//Sul
			if (aux2 - xSize >= 0) {
				if (celulas[aux2].sul == null && (celulas[aux2 - xSize].distancia == valorFim - 1) && (encontrou == false)) {
					aux2 = aux2 - xSize;
					encontrou = true;
					valorFim--;
					celulas [aux2].chao.GetComponent<Renderer> ().material.color = Color.green;
				}
			}
			yield return new WaitForSeconds (velCria);

		}
		menuFim.gameObject.SetActive (true);
		var temp2 = Time.realtimeSinceStartup;
		Debug.Log ("Tempo de execução " + (temp2 - temp) + " segundos");
	}
}