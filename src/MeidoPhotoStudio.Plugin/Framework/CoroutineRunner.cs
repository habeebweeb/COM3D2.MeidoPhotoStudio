namespace MeidoPhotoStudio.Plugin.Framework;

public class CoroutineRunner(Func<IEnumerator> coroutine)
{
    private static GameObject coroutineRunnerParent;

    private Func<IEnumerator> coroutine = coroutine ?? throw new ArgumentNullException(nameof(coroutine));
    private string name;
    private GameObject coroutineContainer;
    private CoroutineBehaviour runnerBehaviour;

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? "[Coroutine Runner]" : value;
    }

    public Func<IEnumerator> Coroutine
    {
        get => coroutine;
        set
        {
            coroutine = value ?? throw new ArgumentNullException(nameof(value));

            Stop();
        }
    }

    public bool Running { get; private set; }

    private static GameObject CoroutineRunnerParent
    {
        get
        {
            if (coroutineRunnerParent)
                return coroutineRunnerParent;

            coroutineRunnerParent = new("[MPS Coroutine Runner Parent]");

            Object.DontDestroyOnLoad(coroutineRunnerParent);

            return coroutineRunnerParent;
        }
    }

    private CoroutineBehaviour RunnerBehaviour
    {
        get
        {
            if (!coroutineContainer)
            {
                coroutineContainer = new GameObject(Name)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };

                coroutineContainer.transform.SetParent(CoroutineRunnerParent.transform);
            }

            if (!runnerBehaviour)
                runnerBehaviour = coroutineContainer.GetOrAddComponent<CoroutineBehaviour>();

            return runnerBehaviour;
        }
    }

    public void Start()
    {
        Stop();

        RunnerBehaviour.StartCoroutine(RunCoroutine());

        IEnumerator RunCoroutine()
        {
            IEnumerator result;

            Running = true;

            try
            {
                result = Coroutine();
            }
            catch
            {
                Object.Destroy(RunnerBehaviour.gameObject);

                Running = false;

                throw;
            }

            yield return result;

            Object.Destroy(RunnerBehaviour.gameObject);

            Running = false;
        }
    }

    public void Stop()
    {
        if (!Running)
            return;

        RunnerBehaviour.StopAllCoroutines();

        Running = false;
    }

    internal static void DestroyParent()
    {
        if (!coroutineRunnerParent)
            return;

        Object.Destroy(coroutineRunnerParent);
    }

    private class CoroutineBehaviour : MonoBehaviour
    {
    }
}
