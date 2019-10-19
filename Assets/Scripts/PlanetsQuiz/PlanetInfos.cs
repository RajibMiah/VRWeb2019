using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlanetInfos : MonoBehaviour {
    private string[] earth;
    private string[] mars;
    private string[] jupiter;
    private string[] saturn;
    private string[] uranus;
    private string[] neptune;
    public TypeWriterText txt_Orbit;
    public TypeWriterText txt_OrbitPeriod;
    public TypeWriterText txt_OrbitalSpeed;
    public TypeWriterText txt_SurfaceTemp;
    public TypeWriterText txt_SurfacePressure;
    public TypeWriterText txt_Composition;
    public TypeWriterText txt_Paragraph;
    public TypeWriterText txt_Distancefromearth;
    private bool shownEarth, shownMars, shownJupter, shownSaturn, shownUranus, shownNeptune, shownJupiter;
    public GameObject panel_Hud;
    private void Start() {
        earth = new string[1];
        mars = new string[1];
        jupiter = new string[1];
        saturn = new string[1];
        neptune = new string[1];
        uranus = new string[1];


    }
    public void SetInfoEarth() {

        earth[0] = "Earth is our home planet. Scientists believe Earth and its moon formed around the same time as the rest of the solar system. They think that was about 4.5 billion years ago. Earth is the fifth-largest planet in the solar system. Its diameter is about 8,000 miles. And Earth is the third-closest planet to the sun. Its average distance from the sun is about 93 million miles. Only Mercury and Venus are closer.";
        // earth[1] = "Earth is our home planet. Scientists believe Earth and its moon formed around the same time as the rest of the solar system. They think that was about 4.5 billion years ago. Earth is the fifth-largest planet in the solar system. Its diameter is about 8,000 miles. And Earth is the third-closest planet to the sun. Its average distance from the sun is about 93 million miles. Only Mercury and Venus are closer.";
        if (shownEarth)
            return;
        shownEarth = true;
        txt_Paragraph.SetNewInfoText(earth);
        txt_Orbit.SetNewInfoText("okay");
        txt_OrbitPeriod.SetNewInfoText("okay");
        txt_OrbitalSpeed.SetNewInfoText("okay");
        txt_SurfaceTemp.SetNewInfoText("okay");
        txt_SurfacePressure.SetNewInfoText("okay");
        txt_Composition.SetNewInfoText("okay");
        txt_Distancefromearth.SetNewInfoText("okay");

    }

    public void SetInfoMars() {
       
        mars[0] = "mars is our home planet. Scientists believe Earth and its moon formed around the same time as the rest of the solar system. They think that was about 4.5 billion years ago. Earth is the fifth-largest planet in the solar system. Its diameter is about 8,000 miles. And Earth is the third-closest planet to the sun. Its average distance from the sun is about 93 million miles. Only Mercury and Venus are closer.";
        if (shownMars)
            return;
        panel_Hud.SetActive(true);
        shownMars = true;
        txt_Paragraph.SetNewInfoText(mars);
        txt_Orbit.SetNewInfoText("okay");
        txt_OrbitPeriod.SetNewInfoText("okay");
        txt_OrbitalSpeed.SetNewInfoText("okay");
        txt_SurfaceTemp.SetNewInfoText("okay");
        txt_SurfacePressure.SetNewInfoText("okay");
        txt_Composition.SetNewInfoText("okay");
        txt_Distancefromearth.SetNewInfoText("okay");
    }

    public void SetInfoJupiter() {
        
        jupiter[0] = "jupiter is our home planet. Scientists believe Earth and its moon formed around the same time as the rest of the solar system. They think that was about 4.5 billion years ago. Earth is the fifth-largest planet in the solar system. Its diameter is about 8,000 miles. And Earth is the third-closest planet to the sun. Its average distance from the sun is about 93 million miles. Only Mercury and Venus are closer.";
        if (shownJupiter)
            return;
        panel_Hud.SetActive(true);
        shownJupiter = true;
        txt_Paragraph.SetNewInfoText(jupiter);
        txt_Orbit.SetNewInfoText("okay");
        txt_OrbitPeriod.SetNewInfoText("okay");
        txt_OrbitalSpeed.SetNewInfoText("okay");
        txt_SurfaceTemp.SetNewInfoText("okay");
        txt_SurfacePressure.SetNewInfoText("okay");
        txt_Composition.SetNewInfoText("okay");
        txt_Distancefromearth.SetNewInfoText("okay");
    }

    public void SetInfoSaturn() {
       
        saturn[0] = "saturn is our home planet. Scientists believe Earth and its moon formed around the same time as the rest of the solar system. They think that was about 4.5 billion years ago. Earth is the fifth-largest planet in the solar system. Its diameter is about 8,000 miles. And Earth is the third-closest planet to the sun. Its average distance from the sun is about 93 million miles. Only Mercury and Venus are closer.";
        if (shownSaturn)
            return;
        shownSaturn = true;
        txt_Paragraph.SetNewInfoText(saturn);
        txt_Orbit.SetNewInfoText("okay");
        txt_OrbitPeriod.SetNewInfoText("okay");
        txt_OrbitalSpeed.SetNewInfoText("okay");
        txt_SurfaceTemp.SetNewInfoText("okay");
        txt_SurfacePressure.SetNewInfoText("okay");
        txt_Composition.SetNewInfoText("okay");
        txt_Distancefromearth.SetNewInfoText("okay");
    }

    public void SetInfoUranus() {
       
        uranus[0] = "uranus is our home planet. Scientists believe Earth and its moon formed around the same time as the rest of the solar system. They think that was about 4.5 billion years ago. Earth is the fifth-largest planet in the solar system. Its diameter is about 8,000 miles. And Earth is the third-closest planet to the sun. Its average distance from the sun is about 93 million miles. Only Mercury and Venus are closer.";
        if (shownUranus)
            return;
        shownUranus = true;
        panel_Hud.SetActive(true);
        txt_Paragraph.SetNewInfoText(uranus);
        txt_Orbit.SetNewInfoText("okay");
        txt_OrbitPeriod.SetNewInfoText("okay");
        txt_OrbitalSpeed.SetNewInfoText("okay");
        txt_SurfaceTemp.SetNewInfoText("okay");
        txt_SurfacePressure.SetNewInfoText("okay");
        txt_Composition.SetNewInfoText("okay");
        txt_Distancefromearth.SetNewInfoText("okay");
    }

    public void SetInfoNeptune() {
        
        neptune[0] = "neptune is our home planet. Scientists believe Earth and its moon formed around the same time as the rest of the solar system. They think that was about 4.5 billion years ago. Earth is the fifth-largest planet in the solar system. Its diameter is about 8,000 miles. And Earth is the third-closest planet to the sun. Its average distance from the sun is about 93 million miles. Only Mercury and Venus are closer.";
        if (shownNeptune)
            return;
        shownNeptune = true;
        panel_Hud.SetActive(true);
        txt_Paragraph.SetNewInfoText(neptune);
        txt_Orbit.SetNewInfoText("okay");
        txt_OrbitPeriod.SetNewInfoText("okay");
        txt_OrbitalSpeed.SetNewInfoText("okay");
        txt_SurfaceTemp.SetNewInfoText("okay");
        txt_SurfacePressure.SetNewInfoText("okay");
        txt_Composition.SetNewInfoText("okay");
        txt_Distancefromearth.SetNewInfoText("okay");
    }
}


