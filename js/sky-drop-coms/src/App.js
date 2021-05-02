import logo from './logo.svg';
import './App.css';
import React from 'react';
import { SkynetClient } from 'skynet-js';

class App extends React.Component 
{
    async componentDidMount()
    {
        let client = new SkynetClient("https://siasky.net");
        let mySky = await client.loadMySky("siasky.net");

        console.log("mySky", mySky);

        const loggedIn = await mySky.checkLogin();
        console.log("loggedIn", loggedIn);

        if (!loggedIn) 
        {
            window.document
                .getElementById("login-button")
                .addEventListener("click", mySky.requestLoginAccess);
        }
    }

    render()
    {
        return (
            <div className="App">
            <header className="App-header">
                <img src={logo} className="App-logo" alt="logo" />
                <button id="login-button">Press me</button>
                <p>
                Edit <code>src/App.js</code> and save to reload.
                </p>
                <a
                className="App-link"
                href="https://reactjs.org"
                target="_blank"
                rel="noopener noreferrer"
                >
                Learn React
                </a>
            </header>
            </div>
        );
    }
}

export default App;
