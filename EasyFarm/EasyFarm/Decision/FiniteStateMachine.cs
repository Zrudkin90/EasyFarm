﻿// Author: Myrmidon
// Site: FFEVO.net
// All credit to him!

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EasyFarm.Engine;
using EasyFarm.FSM;
using System.Threading.Tasks;
using EasyFarm.Decision.FSM;

public class FiniteStateEngine
{
    private List<BaseState> Brains = new List<BaseState>();
    private BaseState LastRan = null;

    // Timer loop, check the State list.
    private Timer Heartbeat = new Timer();
    private GameEngine Engine;


    private Task m_thread;
    public Task Thread
    {
        get
        {
            return 
                m_thread == null || !m_thread.Status.Equals(TaskStatus.Running) ?
                m_thread = new Task(() => Heartbeat_Tick(null, null)) : m_thread;
        }    
    }

    // Constructor.
    public FiniteStateEngine()
    {
        Heartbeat.Interval = 100; // Check State list ten times per second.
        Heartbeat.Tick += new EventHandler((X, Y) => 
        {
            if(Thread.Status != TaskStatus.Running) 
                Thread.Start(); 
        });
    }

    public FiniteStateEngine(ref GameEngine Engine)
        : this()
    {
        this.Engine = Engine;

        //Create the states
        AddState(new RestState(ref this.Engine));
        AddState(new AttackState(ref this.Engine));
        AddState(new TravelState(ref this.Engine));
        AddState(new HealingState(ref this.Engine));
        AddState(new DeadState(ref this.Engine));

        foreach (var b in this.Brains)
            b.Enabled = true;
    }

    // Handles the updating.
    public void Heartbeat_Tick(object sender, EventArgs e)
    {
        lock (Brains)
        {
            // Sort the List, States may have updated Priorities.
            Brains.Sort();

            // Find a State that says it needs to run.
            foreach (BaseState BS in Brains)
            {
                if (BS.Enabled == false) { continue; } // Skip disabled States.
                if (BS.CheckState() == true)
                {
                    // Says it needs to run. Same State as before?
                    if (LastRan == null) { LastRan = BS; }
                    if (LastRan != BS)
                    {
                        // Make the previous State clean up and exit.
                        LastRan.ExitState();
                        LastRan = BS;
                        BS.EnterState();
                    }

                    // Run this State and stop.
                    BS.RunState();
                    return;
                }
            }
        }
    }

    // Start and stop.
    public void Start() { Heartbeat.Start(); }
    public void Stop() { Heartbeat.Stop(); }

    // Add and remove States.
    public void AddState(BaseState ToAdd) { lock (Brains) { Brains.Add(ToAdd); } }
    public void RemoveState(int index) { lock (Brains) { Brains.RemoveAt(index); } }
}