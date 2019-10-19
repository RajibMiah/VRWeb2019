using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour {
    public float secondsForOneLength = 1f;
    public float upPos = 30;

    private Vector3 startPos, endPos;
    //private Color startColor,endColor;
    private RectTransform rectTransform;
    private Text txt;
    public QuestionCollector[] questions = new QuestionCollector[3];

    private void Start() {
        rectTransform = GetComponent<RectTransform>();
        //txt = GetComponent<Text>();
        startPos = rectTransform.anchoredPosition;
        endPos = startPos - Vector3.up * upPos;
        //startColor = txt.color;
        //endColor = new Color(startColor.r, startColor.g, startColor.b, 0);
     
    }


    private void Update() {
        float damp = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(Time.time / secondsForOneLength, 1f));
        rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos,damp);
        //txt.color = Color.Lerp(startColor, endColor, damp);
    }



}

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            