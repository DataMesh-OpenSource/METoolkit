using UnityEngine;
using System.Collections;

[AddComponentMenu("Effects/SetSortingOrder")]

[ExecuteInEditMode]
public class SetSortingOrder : MonoBehaviour
{
	public string sortingLayer = "Default";
    public int Order = 0;

    public bool beFitForParentsOrder = true;
    public bool CanChangeAllRendererSortingOrder = false;
    public bool CanChangeAllParticleSortingOrder = false;
    public bool CanChangeAllCanvasSortingOrder = false;
    public bool CanChangeThisRendersoringOrder = false;
    public bool CanChangeThisParticleSortingOrder = false;
    public bool CanChangeThisCanvasOrder = false;

    public bool AlwaysKeepOrder = false;
	
    void Update()
    {
        if (AlwaysKeepOrder||!Application.isPlaying)
        {
           InitSort();
        }
    }

	private void SortAll()
    {
		if (CanChangeAllRendererSortingOrder){
			Renderer[] mrs = GetComponentsInChildren<Renderer>(true);
			for (int i = 0; i < mrs.Length; i++){
				Renderer mr = mrs[i];
				mr.sortingLayerID = SortingLayer.NameToID(sortingLayer);
				mr.sortingOrder = Order;
			}
		}else if (CanChangeThisRendersoringOrder){
			Renderer mr = GetComponent<Renderer>();
			if (mr != null){
				mr.sortingLayerID = SortingLayer.NameToID(sortingLayer);
				mr.sortingOrder = Order;
			}

		}

		if (CanChangeAllParticleSortingOrder){
			ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < particles.Length; i++){
				particles[i].GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID(sortingLayer);
				particles[i].GetComponent<Renderer>().sortingOrder = Order;
			}
		}else if (CanChangeThisParticleSortingOrder){
			ParticleSystem particle = GetComponent<ParticleSystem>();
			if (particle != null){
				particle.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID(sortingLayer);
				particle.GetComponent<Renderer>().sortingOrder = Order;
			}
		}

		if (CanChangeAllCanvasSortingOrder){
			Canvas[] panels = GetComponentsInChildren<Canvas>(true);
			for (int i = 0; i < panels.Length; i++)
            {
				panels[i].sortingLayerName = sortingLayer;
				panels[i].sortingOrder = Order;
			}
		}else if (CanChangeThisCanvasOrder){
            Canvas panel = GetComponent<Canvas>();
			if (panel != null)
            { 
				panel.sortingLayerName = sortingLayer;
				panel.sortingOrder = Order;
			}
		}
    }

    public void InitSort()
    {
        SortAll();
    }
    
    private void Start()
    {
        InitSort();
    }

    private void OnEnable()
    {
        InitSort();
    }
}