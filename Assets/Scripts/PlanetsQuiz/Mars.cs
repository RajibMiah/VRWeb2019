using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mars : MonoBehaviour
{
    public QuestionCollector[] questions = new QuestionCollector[4];
    public int maxQuestions = 3;


    public void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            if (other.gameObject.GetComponent<Voyger2SpeedCrontroll>() != null) {
                other.gameObject.GetComponent<Voyger2SpeedCrontroll>().speed = 0;
                other.gameObject.GetComponent<Voyger2SpeedCrontroll>().enabled = false;
            }
            if (other.gameObject.GetComponent<Voyger1SpeedControl>() != null) {
                other.gameObject.GetComponent<Voyger1SpeedControl>().speed = 0;
                other.gameObject.GetComponent<Voyger1SpeedControl>().enabled = false;
            }
            QuestionTemplate();
        }

    }

    public void QuestionTemplate() {
        questions[0].Questions = "What is your name?";
        questions[0].A = "Shanta2";
        questions[0].B = "Shafayet2";
        questions[0].C = "Rajib2";
        questions[0].D = "Dimik2";
        questions[0].correctAns = "Shanta2";
        questions[1].Questions = "Which Mars is for human?";
        questions[1].A = "Earth2";
        questions[1].B = "Mars2";
        questions[1].C = "Venus2";
        questions[1].D = "Saturn2";
        questions[1].correctAns = "Earth2";
        questions[2].Questions = "From where do we receive Light?";
        questions[2].A = "Sun2";
        questions[2].B = "Moon2";
        questions[2].C = "Saturn2";
        questions[2].D = "Pluto2";
        questions[2].correctAns = "Sun2";

        QuizManager.instance.maxQuestions = maxQuestions;
        QuizManager.instance.ReceiveQuiz(questions);
    }
}
