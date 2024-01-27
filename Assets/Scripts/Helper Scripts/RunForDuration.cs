using UnityEngine;
using System;

public class RunForDuration : CustomYieldInstruction {

    // ---

    protected float duration;

    protected bool inverted;
    protected float t;

    // ---

    public delegate void ProgressEvent (float _nt);
    protected ProgressEvent progressEvent;

    // ---

    protected Func<float> timeScaleFunc;

    // ---

    public override bool keepWaiting {
        get {

            if (t >= duration) {
                t -= duration;
                return false;
            }

            t += Time.unscaledDeltaTime * timeScaleFunc();

            if (t >= duration) {

                progressEvent(inverted ? 0f : 1f);

            } else {

                float nt = Mathf.Clamp01(t / duration);
                if (inverted) nt = 1f - nt;
                progressEvent(nt);

            }

            return true;

        }
    }

    public RunForDuration (float _duration, ProgressEvent _progressEvent, Func<float> _timeScaleFunc = null) {

        progressEvent = _progressEvent;
        timeScaleFunc = _timeScaleFunc ?? (() => Time.timeScale);
        duration = _duration;
        inverted = false;
        t = 0;

    }

    public RunForDuration Get (bool _inverted = false) {

        inverted = _inverted;
        t = 0;
        return this;

    }

}