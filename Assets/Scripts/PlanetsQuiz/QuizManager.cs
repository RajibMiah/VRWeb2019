using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public class QuestionCollector {
    public string Questions;
    public string A;
    public string B;
    public string C;
    public string D;
    public string correctAns;

}

public class QuizManager : MonoBehaviour
{
    public Button btn_A, btn_B, btn_C, btn_D;
    private Text txt_A, txt_B, txt_C, txt_D;
    private Image img_A, img_B, img_C, img_D;
    public Text txt_Question;
    public static QuizManager instance;
    public QuestionCollector[] QuestionList;
    public GameObject canvas_Quiz;
    public GameObject panel_PlanetInfo;
    int currentQuestion = 0;
    private Color correctColor = Color.green, wrongColor = Color.red;
    private Color startColor;
    private Color highlightColor;
    public int maxQuestions = 5;
    public Voyger1SpeedControl voyager1Ctrl;
    public Voyger2SpeedCrontroll voyager2Ctrl;
    private float lastTimeAnswered;
    private float waitDelay = 2f;
    public RectTransform HealthRect;
    public Text health;
    private float maxHealth = 100;
    private float currentHealth = 100;
    public GameObject Canvas_Container_Buttons;
    public GameObject panel_Quiz;
    public GameObject panel_VoyagerDestroyed;
    public Text panel_VoyagerDestroyedDescription;
    [HideInInspector]
    public bool LastPlanet;
    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start() {

        txt_A = btn_A.transform.GetChild(0).GetComponent<Text>();
        txt_B = btn_B.transform.GetChild(0).GetComponent<Text>();
        txt_C = btn_C.transform.GetChild(0).GetComponent<Text>();
        txt_D = btn_D.transform.GetChild(0).GetComponent<Text>();
        img_A = btn_A.transform.GetComponent<Image>();
        img_B = btn_B.transform.GetComponent<Image>();
        img_C = btn_C.transform.GetComponent<Image>();
        img_D = btn_D.transform.GetComponent<Image>();
        startColor = btn_A.GetComponent<Image>().color;
        lastTimeAnswered = Time.time;

    }


    public void ActivateQuizPanel() {
        /*canvasGroup_Quiz.interactable = true;
        canvasGroup_Quiz.blocksRaycasts = true;*/
        panel_PlanetInfo.SetActive(false);
        currentQuestion = 0;
        ResetQuizPanel();
        canvas_Quiz.SetActive(true);
        btn_A.onClick.AddListener(delegate { SetBtnAnswer(QuestionList[currentQuestion].A, img_A); });
        btn_B.onClick.AddListener(delegate { SetBtnAnswer(QuestionList[currentQuestion].B, img_B); });
        btn_C.onClick.AddListener(delegate { SetBtnAnswer(QuestionList[currentQuestion].C, img_C); });
        btn_D.onClick.AddListener(delegate { SetBtnAnswer(QuestionList[currentQuestion].D, img_D); });
    }

    public void GiveQuestion() {
        if (currentQuestion >= maxQuestions) {
            currentQuestion = 0;
            ResetQuizPanel();
            lastTimeAnswered = Time.time;
            canvas_Quiz.SetActive(false);
            if (LastPlanet) {
                Canvas_Container_Buttons.SetActive(false);
                if (voyager2Ctrl != null) {
                    panel_VoyagerDestroyedDescription.text = "Congratulations! You completed Nasa's Voyager 2 Journey!";
                }
                else if (voyager1Ctrl != null) {
                    panel_VoyagerDestroyedDescription.text = "Congratulations! You completed Nasa's Voyager 1 Journey!";
                }
                panel_VoyagerDestroyed.SetActive(true);
                return;
            }
               
            if (voyager2Ctrl != null) {
                voyager2Ctrl.enabled = true;
                voyager2Ctrl.speed = voyager2Ctrl.whenOutOfCollider;
            }
            else if (voyager1Ctrl != null) {
                voyager1Ctrl.enabled = true;
                voyager1Ctrl.speed = voyager1Ctrl.whenOutOfCollider;

            }
           
            return;
        }
         //ResetQuizPanel();
        txt_Question.text = QuestionList[currentQuestion].Questions;
        txt_A.text = QuestionList[currentQuestion].A;
        txt_B.text = QuestionList[currentQuestion].B;
        txt_C.text = QuestionList[currentQuestion].C;
        txt_D.text = QuestionList[currentQuestion].D;
     
    }

    public void ReceiveQuiz(QuestionCollector[] newQuestionList) {

        lastTimeAnswered = Time.time;
        QuestionList = new QuestionCollector[newQuestionList.Length+1];
        QuestionList = newQuestionList;
        Shuffle(QuestionList);
    }


    void Shuffle(QuestionCollector[] a) {
        // Loops through array
        for (int i = a.Length - 1-1; i > 0; i--) {
            // Randomize a number between 0 and i (so that the range decreases each time)
            SuffleAnswers(i);
            int rnd = UnityEngine.Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            QuestionCollector temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
           
        }

        /* // Print
         for (int i = 0; i < a.Length; i++) {
             Debug.Log(a[i]);
         }*/
        GiveQuestion();
        Invoke("ActivateQuizPanel", 1f);
    }

    void Shuffle(string[] a, int pos) {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--) {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = UnityEngine.Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            string temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }

        QuestionList[pos].A = a[0];
        QuestionList[pos].B = a[1];
        QuestionList[pos].C = a[2];
        QuestionList[pos].D = a[3];

    }
    public void SetBtnAnswer(string answer, Image img) {
        if (Time.time - lastTimeAnswered >= waitDelay) {
            lastTimeAnswered = Time.time;
            //canvasGroup_Quiz.interactable = false;
            //canvasGroup_Quiz.blocksRaycasts = false;
            if (answer == QuestionList[currentQuestion].correctAns) {
                img.color = correctColor;
            }
            else {
                ReceiveVoyagerDamage();
                img.color = wrongColor;
            }
        
            Invoke("ResetQuizPanel", 0.6f);
            Invoke("GiveQuestion", 2f);
            currentQuestion++;
        }
      
    }

   
    public void ResetQuizPanel() {

        img_A.color = startColor;
        img_B.color = startColor;
        img_C.color = startColor;
        img_D.color = startColor;
        btn_A.interactable = true;
        btn_B.interactable = true;
        btn_C.interactable = true;
        btn_D.interactable = true;
       
    }

    public void SuffleAnswers(int pos) {
        string[] answers = new string[4];
        answers[0] = QuestionList[pos].A;
        answers[1] = QuestionList[pos].B;
        answers[2] = QuestionList[pos].C;
        answers[3] = QuestionList[pos].D;
        Shuffle(answers, pos);

      
    }

    public void ReceiveVoyagerDamage() {
        currentHealth -= 10;
        if (currentHealth <= 0) {
            currentHealth = 0;
            panel_Quiz.SetActive(false);
            panel_VoyagerDestroyed.SetActive(true);
            Canvas_Container_Buttons.SetActive(false);
        }
           
        float newVal = (float)currentHealth / (float)maxHealth;
        health.text = "Health: " + currentHealth + "%";
        HealthRect.localScale = new Vector3(newVal, HealthRect.localScale.y, HealthRect.localScale.z);
    }

   

}
