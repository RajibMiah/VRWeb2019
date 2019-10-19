using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SmoothFocus : MonoBehaviour
{

    public Transform targetObj;
    public Transform Player;
    public Animator anim;
    public RuntimeAnimatorController[] animators;
    private Transform lastTarget;
    public Transform[] partTransforms;
    public float speed = 5;
    public float delay = 5;
    public Vector3 offset;
    public Transform CameraTransform;
    private Vector3 lastPos;
    private Quaternion lastRot;
    private float LastChangeTime;
    private Quaternion offsetRot;
    public Image[] typeButtons;
    public Image[] opticButtons;
    public Image[] instrumentsButtons;
    public Image[] systemButtons;
    private Image lastClickedButton;
    private Color clickColor;
    private Color unClickColor;
    public Text titleText;
    public Text informationText;

    private string s;
    private string title;
    private int focusPart;
    public GameObject canvasInformationPart;
    public GameObject[] panel_types;
    private bool isZoomOut = true;
    private int currentPart = 0;
    private int maxPartsofCurrentType;
    private int maxType = 3;
    private int currentType = 0;

    private Color unClickTypeColor;
    private Color clickTypeColor;
    private MouseOrbitImproved orbit;

    public void Start() {

        lastTarget = targetObj;

        LastChangeTime = Time.time;
        //offsetRot = transform.rotation * Quaternion.Inverse(targetObj.rotation);
        lastClickedButton = opticButtons[0];
        unClickColor = opticButtons[0].color;
        clickColor = Color.green;
        clickTypeColor = typeButtons[0].color;
        unClickTypeColor = typeButtons[1].color;
        anim.runtimeAnimatorController = animators[0];
        anim.SetInteger("Anim", 0);
        canvasInformationPart.SetActive(false);
        title = "Controller Manual is Behind you";
        s = "Press 2Key / look at Click Me Button to view Information. Use Joystick to rotate and Press C & D to Rotate around";
        informationText.text = s;
        titleText.text = title;
        orbit = Player.GetComponent<MouseOrbitImproved>();
        orbit.enabled = isZoomOut;

    }

    void  Update() {

        if (Input.GetButtonDown("Click") || Input.GetKeyDown(KeyCode.Escape)) {
            ToggleZoom();
        }
        if (!isZoomOut) {
            if (Input.GetButtonDown("A") || Input.GetKeyDown(KeyCode.A)) {
                currentPart++;
                MoveThroughParts();
            }
            else if (Input.GetButtonDown("B") || Input.GetKeyDown(KeyCode.B)) {
                currentPart--;
                MoveThroughParts();
            }
            else if (Input.GetButtonDown("C") || Input.GetKeyDown(KeyCode.C)) {
                MoveThroughType();
            }
           
        }
        targetObj.localPosition = Vector3.Lerp(targetObj.localPosition, partTransforms[focusPart].localPosition, speed * Time.deltaTime);
        targetObj.localRotation = Quaternion.Lerp(targetObj.localRotation, partTransforms[focusPart].localRotation, speed * Time.deltaTime);
    }

    public void FocusOptics() {
        anim.runtimeAnimatorController = animators[0];
        typeButtons[0].color = clickTypeColor;
        typeButtons[1].color = unClickTypeColor;
        typeButtons[2].color = unClickTypeColor;
        currentPart = 0;
        currentType = 0;
        maxPartsofCurrentType = opticButtons.Length;
        OpticsPrimaryMirror();
    }

    public void FocusInstruments() {
        anim.runtimeAnimatorController = animators[1];
        typeButtons[1].color = clickTypeColor;
        typeButtons[0].color = unClickTypeColor;
        typeButtons[2].color = unClickTypeColor;
        currentPart = 0;
        currentType = 1;
        maxPartsofCurrentType = instrumentsButtons.Length;
        InstrumentMIRI();
    }

    public void FocusSystems() {
        anim.runtimeAnimatorController = animators[2];
        typeButtons[2].color = clickTypeColor;
        typeButtons[1].color = unClickTypeColor;
        typeButtons[0].color = unClickTypeColor;
        currentPart = 0;
        currentType = 2;
        maxPartsofCurrentType = systemButtons.Length;
        SystemSunshield();
    }
    public void OpticsPrimaryMirror() {
        focusPart = 1;
        currentPart = 0;
        lastClickedButton.color = unClickColor;
        opticButtons[focusPart-1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = opticButtons[focusPart-1];
        title = "Primary Mirror";
        s = "The Primary Mirror is made up of 18 hexagonal segments that work together as a single, 6.5-meter mirror. The Mirror segments are made of beryllium, a very lightweight and strong material. Each mirror segmentis mounted on a hexapod with actuators that enable fine adjustments to each segment in six degrees of freedom: x and y position, piston, tip, tilt, and clocking.An additional actuator at the center of each primary mirror segment provids radius of curvature control. This system enables controlls to finely tune all 18 segments to work as one large mirror.";
        informationText.text = s;
        titleText.text = title;
    }

    public void OpticsSecondaryMirror() {
        focusPart = 2;
        currentPart = 1;
        lastClickedButton.color = unClickColor;
        opticButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = opticButtons[focusPart - 1];
        title = "Secondary Mirror";
        s = "The Secondary Mirror directs the light from the Primary Mirror to where it can be collected by the Webb's instruments. The secondary mirror is moveable and can be adjusted to focus the telescope.";
        informationText.text = s;
        titleText.text = title;
    }

    public void OpticsSecondaryMirrorSupportStructure() {
        focusPart = 3;
        currentPart = 2;
        lastClickedButton.color = unClickColor;
        opticButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = opticButtons[focusPart - 1];
        title = "Secondary Mirror Support Structure";
        s = "The Secondary Mirror Support Structure deploys and supports the Secondary Mirror.";
        informationText.text = s;
        titleText.text = title;
    }

    public void OpticsAftOpticSubSystem() {
        focusPart = 4;
        currentPart = 3;
        lastClickedButton.color = unClickColor;
        opticButtons[3].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = opticButtons[focusPart - 1];
        title = "Aft-Optics Subsystem";
        s = "The Aft-Optics Subsystem contains the fixed tertiary mirror and the fine steering mirror. The Aft-Optics Subsystem's most prominent feature is a central baffle protruding from the center of the Primary Mirror. This baffle prevents stray light from entering Webb's optics.";
        informationText.text = s;
        titleText.text = title;
    }

    public void OpticsBackplane() {
        focusPart = 5;
        currentPart = 4;
        lastClickedButton.color = unClickColor;
        opticButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = opticButtons[focusPart - 1];
        title = "Backplane";
        s = "The Backplane structure holds the Primary Mirror segments in place and keeps them steady. It supports 7,500 lbs (2400 kg) of telescope optics and instruments in the ISIM structure.";
        informationText.text = s;
        titleText.text = title;
    }

    public void InstrumentMIRI() {
        focusPart = 1;
        currentPart = 0;
        lastClickedButton.color = unClickColor;
        instrumentsButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = instrumentsButtons[focusPart - 1];
        title = "Mid-Infrared Instrument (MIRI)";
        s = "The Mid-Infrared Instrument contains state-of-the-art Infrared detector arrays that will help make important contributions to all four of the mission design thems for the Webb.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 6;
    }

    public void InstrumentNIRCam() {
        focusPart = 2;
        currentPart = 1;
        lastClickedButton.color = unClickColor;
        instrumentsButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = instrumentsButtons[focusPart - 1];
        title = "Near-Infrared Camera (NIRCam)";
        s = "The Near-Infrared Camera is Webb's primary imager. It is optimized to detect the first light-emitting  galaxies and star clusters to form in the Universe after the Big Bang. The NIRCam includes features that make it a powerful tool for studying star formation in our Milky Way Galaxy and for discovering and characterizing planes around other stars. NIRCam is also a wavefront sensor enabling precise alignment of the primary mirror.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 6;
    }

    public void InstrumentNIRSpec() {
        focusPart = 3;
        currentPart = 2;
        lastClickedButton.color = unClickColor;
        instrumentsButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = instrumentsButtons[focusPart - 1];
        title = "The Near-Infrared Spectrometer (NIRSpec)";
        s = "The Near-Infrared Spectrometer provides much of the physics information about celestial objects. Its unique design enables observations of a hundred objects simultaneously. NIRSpec will perform large surveys of faint galaxies and help determine the molecular composition, metallicity, star formation rate and distance of the galaxies.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 6;
    }

    public void InstrumentFGSOrTFI() {
        focusPart = 4;
        currentPart = 3;
        lastClickedButton.color = unClickColor;
        instrumentsButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = instrumentsButtons[focusPart - 1];
        title = "Fine Guidance Sensor / Tunable Filter Imager (FGS/TFI)";
        s = "The Fine Guidance Sensor Tunable Filter Imager is actually two instruments in one. The Fine Guidance Sensor Consists of two specialized cameras that work like a guiding scope enabling the Webb Telescope to locate its celestial targets, determine its own position and remain pointed as n object so that the telescope can collect high-quality data.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 6;
    }

    public void InstrumentIntegratedScieneceInstrumentModule() {
        focusPart = 5;
        currentPart = 4;
        lastClickedButton.color = unClickColor;
        instrumentsButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = instrumentsButtons[focusPart - 1];
        title = "Integrated Scienece Instrument Module";
        s = "The Integrated Science Instrument Module is the structure that holds Webb's four science Instruments.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 6;
    }

    public void SystemSunshield() {
        focusPart = 1;
        currentPart = 0;
        lastClickedButton.color = unClickColor;
        systemButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = systemButtons[focusPart - 1];
        title = "Sunshield";
        s = "The sunshield's five, Kapton based layers keep the infrared light )or heatr) from the sun, Earth and moon, as well as the spacecraft bus electronics, from reaching Webb's mirrors and science instruments";
        informationText.text = s;
        titleText.text = title;
        focusPart = 7;
    }

    public void SystemUnitizedPalletStructure() {
        focusPart = 2;
        currentPart = 1;
        lastClickedButton.color = unClickColor;
        systemButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = systemButtons[focusPart - 1];
        title = "Unitized Pallet Structure";
        s = "The Unitized Pallet Structure holds the rolled-up sunshield during launch.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 8;
    }

    public void SystemSolarPanels() {
        focusPart = 3;
        currentPart = 2;
        lastClickedButton.color = unClickColor;
        systemButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = systemButtons[focusPart - 1];
        title = "Solar Panels";
        s = "The Solar Panels convert sunlight into the power needed to operate the science instruments and the spacecraft subsystems.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 9;
    }

    public void SystemHighGainAntennar() {
        focusPart = 4;
        currentPart = 4;
        lastClickedButton.color = unClickColor;
        systemButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = systemButtons[focusPart - 1];
        title = "Star Trackers";
        s = "The Star Trackers use guide stars for coarse pointing of the telescope. The star tracker data enables the alignment and control system to point the telescope so that the target appears in the field of view of the intended instrument. Once an observation is started, the Fine Guidance Sensor (located in the ISIM with the instruments) can compensate for small drifts in the observatory's alignment and help the telescope maintain its good pointing.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 10;
    }

    public void SystemStarTrackers() {
        focusPart = 5;
        currentPart = 5;
        lastClickedButton.color = unClickColor;
        systemButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = systemButtons[focusPart - 1];
        title = "Momentum Trim Tab";
        s = "The Momentum Trim Tab balances the pressure exerted on Webb's sunshield by sunlight (photons). It works like a trim flap in sailing. The Momentum Trim Tab is not adjustable on orbit.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 11;
    }

    public void SystemMomentumTrimTab() {
        focusPart = 6;
        currentPart = 6;
        lastClickedButton.color = unClickColor;
        systemButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = systemButtons[focusPart - 1];
        title = "Primary Mirror";
        s = "The Primary Mirror is made up of 18 hexagonal segments that work together as a single, 6.5-meter mirror. The Mirror segments are made of beryllium, a very lightweight and strong material. Each mirror segmentis mounted on a hexapod with actuators that enable fine adjustments to each segment in six degrees of freedom: x and y position, piston, tip, tilt, and clocking.An additional actuator at the center of each primary mirror segment provids radius of curvature control. This system enables controlls to finely tune all 18 segments to work as one large mirror.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 12;
    }

    public void SystemSpacecraftBus() {
        focusPart = 7;
        currentPart = 3;
        lastClickedButton.color = unClickColor;
        systemButtons[focusPart - 1].color = clickColor;
        anim.SetInteger("Anim", focusPart);
        lastClickedButton = systemButtons[focusPart - 1];
        title = "Spacecraft Bus";
        s = "The Spacecraft Bus provides the necessary support functions for the operation of the observatory. It contains six major spacecraft subsystems: Electrical Power Subsystem, Attitude Control Subsystem, Communication Subsystem, Command and Data Handling Subsystem, Propulsion Subsystem, and the Thermal Control Subsystem.";
        informationText.text = s;
        titleText.text = title;
        focusPart = 13;
    }

    public void ToggleZoom() {
        if (isZoomOut) {
            Player.transform.position = Vector3.zero;
            Player.transform.rotation = Quaternion.identity;
            isZoomOut = false;

        }
        else {
            isZoomOut = true;
            Player.transform.position = new Vector3(-1.117f, 1.65f, 5.438999f);
            Player.transform.rotation = Quaternion.identity;
        }
        orbit.enabled = isZoomOut;
        if (isZoomOut) {
            focusPart = 0;
            anim.SetInteger("Anim", focusPart);
            anim.SetInteger("Anim", 0);
            anim.runtimeAnimatorController = animators[0];
            title = "Controller Manual is Behind you";
            s = "Press 2Key to Zoom in to see information or Zoom out to cancel rotate JWST.";
            informationText.text = s;
            titleText.text = title;
            canvasInformationPart.SetActive(false);
            panel_types[currentType].SetActive(false);
        }
        else {
            currentType = 0;
            panel_types[currentType].SetActive(true);
            canvasInformationPart.SetActive(true);
            FocusOptics();

        }
    }

    public void MoveThroughParts() {
        if (currentPart >= maxPartsofCurrentType)
            currentPart = 0;
        else if(currentPart< 0) {
            currentPart = maxPartsofCurrentType - 1;
        }
        if (currentType == 0) {
           /* if (currentPart == 0)
                OpticsPrimaryMirror();
            else if (currentPart == 1)
                OpticsSecondaryMirror();
            else if (currentPart == 2)
                OpticsSecondaryMirrorSupportStructure();
            else if (currentPart == 3)
                OpticsAftOpticSubSystem();
            else if (currentPart == 4)
                OpticsBackplane();*/
            switch (currentPart) {
                case 0:
                    OpticsPrimaryMirror();
                    break;
                case 1:
                    OpticsSecondaryMirror();
                    break;
                case 2:
                    OpticsSecondaryMirrorSupportStructure();
                    break;
                case 3:
                    OpticsAftOpticSubSystem();
                    break;
                case 4:
                    OpticsBackplane();
                    break;
                default:
                    return;
            }
        }
        if (currentType == 1) {
            /*if (currentPart == 0)
                InstrumentMIRI();
            else if (currentPart == 1)
                InstrumentNIRCam();
            else if (currentPart == 2)
                InstrumentNIRSpec();
            else if (currentPart == 3)
                InstrumentFGSOrTFI();
            else if (currentPart == 4)
                InstrumentIntegratedScieneceInstrumentModule();*/
            switch (currentPart) {
                case 0:
                    InstrumentMIRI();
                    break;
                case 1:
                    InstrumentNIRCam();
                    break;
                case 2:
                    InstrumentNIRSpec();
                    break;
                case 3:
                    InstrumentFGSOrTFI();
                    break;
                case 4:
                    InstrumentIntegratedScieneceInstrumentModule();
                    break;
                default:
                    return;
            }
        }

        if (currentType == 2) {
            switch (currentPart){
                case 0:
                    SystemSunshield();
                            break;
                case 1:
                    SystemUnitizedPalletStructure();
                    break;
                case 2:
                    SystemSolarPanels();
                    break;
                case 3:
                    SystemSpacecraftBus();
                    break;
                case 4:
                    SystemHighGainAntennar();
                    break;
                case 5:
                    SystemStarTrackers();
                    break;
                case 6:
                    SystemMomentumTrimTab();
                    break;
                default:
                    return;
        }
        /*if (currentPart == 0)
                SystemSunshield();
            else if (currentPart == 1)
                SystemUnitizedPalletStructure();
            else if (currentPart == 2)
                SystemSolarPanels();
            else if (currentPart == 3)
                SystemSpacecraftBus();
            else if (currentPart == 4)
                SystemHighGainAntennar();
            else if (currentPart == 5)
                SystemStarTrackers();
            else if (currentPart == 6)
                SystemMomentumTrimTab();*/
        }
    }

    public void MoveThroughType() {
        panel_types[currentType].SetActive(false);
        currentType++;
        if (currentType >= maxType)
            currentType = 0;
        if (currentType == 0)
            FocusOptics();
        else if (currentType == 1)
            FocusInstruments();
        else if (currentType == 2)
            FocusSystems();
        panel_types[currentType].SetActive(true);
    }
}
