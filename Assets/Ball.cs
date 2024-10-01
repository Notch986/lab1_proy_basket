using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ball : MonoBehaviour
{
    public float force = 100f;
    public GameObject ballPrediction;
    public int maxTrajectoryIteration = 50;

    private Vector2 defaultBallPosition;
    private Vector2 startPosition;
    private Rigidbody2D physics;

    private Scene sceneMain;
    private PhysicsScene2D sceneMainPhysics;
    private Scene scenePrediction;
    private PhysicsScene2D scenePredictionPhysics;

    void Awake(){
        physics = GetComponent<Rigidbody2D>();
    }

    void Start()
    {

        Physics2D.simulationMode = SimulationMode2D.Script;

        physics.isKinematic = true;
        defaultBallPosition = transform.position;

        createSceneMain();
        createScenePrediction();                
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            startPosition = getMousePosition();
        }

        if(Input.GetMouseButton(0)){
            GameObject newBallPrediction = spawnBallPrediction();
            throwBall(newBallPrediction.GetComponent<Rigidbody2D>());

            createTrajectory(newBallPrediction);

            Destroy(newBallPrediction);
        }

        if(Input.GetMouseButtonUp(0)){
            GetComponent<LineRenderer>().positionCount = 0;
            physics.isKinematic = false;

            throwBall(physics);
        }
    }

    void FixedUpdate()
    {
        if (!sceneMainPhysics.IsValid()) return;

        sceneMainPhysics.Simulate(Time.fixedDeltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.tag.Equals("ground")) return;

        physics.isKinematic = true;
        transform.position = defaultBallPosition;
        physics.velocity = Vector2.zero;
        physics.angularVelocity = 0f;
    }

    private void createTrajectory(GameObject newBallPrediction){
        LineRenderer ballLine = GetComponent<LineRenderer>();
        ballLine.positionCount = maxTrajectoryIteration;

        for (int i = 0; i < maxTrajectoryIteration; i++)
        {
            scenePredictionPhysics.Simulate(Time.fixedDeltaTime);
            ballLine.SetPosition(i, new Vector3(newBallPrediction.transform.position.x, newBallPrediction.transform.position.y, 0));
        }
    }

    private void throwBall(Rigidbody2D physics){
        physics.AddForce(getThrowPower(startPosition, getMousePosition()), ForceMode2D.Force);
    }

    private GameObject spawnBallPrediction(){
        GameObject newBallPrediction = GameObject.Instantiate(ballPrediction);
        SceneManager.MoveGameObjectToScene(newBallPrediction, scenePrediction);
        newBallPrediction.transform.position = transform.position;

        return newBallPrediction;
    }

    private Vector2 getThrowPower(Vector2 startPosition, Vector2 endPosition){
        return (startPosition - endPosition) * force;
    }

    private Vector2 getMousePosition(){
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void createSceneMain()
    {
        sceneMain = SceneManager.CreateScene("MainScene");
        sceneMainPhysics = sceneMain.GetPhysicsScene2D();
    }

    private void createScenePrediction()
    {
        CreateSceneParameters sceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        scenePrediction = SceneManager.CreateScene("PredictionScene", sceneParameters);
        scenePredictionPhysics = scenePrediction.GetPhysicsScene2D();
    }
}
