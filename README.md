# 🥫 AR-Food-Package

An Augmented Reality (AR) project that enhances food packaging by enabling interactive, immersive, and AI-powered features using Unity, Vuforia, MediaPipe, and real-time APIs.

## ✨ Overview

**AR-Food-Package** is an innovative system that transforms static food packaging into an intelligent and interactive experience. It brings together real-time 3D graphics, gesture-based interaction, voice AI, and contextual awareness to redefine how users access and engage with product information.

Key technologies:
- **Unity3D**
- **Vuforia Engine**
- **MediaPipe Unity Plugin**
- **Google Cloud Speech APIs**
- **Gemini AI**
- **Open-Meteo API**

## 🎯 Features

- **📦 Marker-Based AR**  
  Scan food packages using image targets to visualize dynamic 3D content and user interfaces directly on the packaging.

- **🧬 Molecular Visualization**  
  Animated 3D models of nutrients (e.g., proteins, vitamins) allow users to rotate, zoom, and explore food chemistry interactively.

- **🌍 Ingredient-Origin Globe**  
  A rotating globe highlights the sourcing regions of key ingredients, promoting transparency and traceability.

- **🧠 AI-Powered Recommendations**  
  Integrates voice-to-text input with Gemini API to generate dietary suggestions personalized to current weather and nutritional data.

- **💬 Voice Interaction & TTS**  
  Use natural language to query information and receive spoken responses, enhancing accessibility and engagement.

- **🖐️ Hand Tracking**  
  MediaPipe detects fingertip positions for touchless interaction with virtual buttons on the package surface.

- **🌤️ Context Awareness**  
  Real-time weather and time data influence AI responses (e.g., suggesting hot drinks on cold days), displayed as 3D text.

## 🛠️ How It Works

- Vuforia tracks markers and places AR content.
- MediaPipe hand tracking provides virtual button interaction.
- Voice input is transcribed using Google Cloud Speech-to-Text.
- Gemini AI processes prompts and weather data to generate suggestions.
- Unity renders and animates 3D models and UI elements.
- Audio responses are provided using Google TTS.

## 📊 Performance Benchmarks

| Scenario                        | iOS (FPS) | Android (FPS) |
|--------------------------------|-----------|----------------|
| No hands, no image target      | 40        | 31             |
| Detected 1 hand                | 39        | 32             |
| Detected 2 hands               | 39        | 29             |
| Pressing voice button          | 26        | 28             |
| Displaying molecule/globe      | 35        | 29             |

> Consistently maintained >30 FPS across devices.

## 🚀 Setup

1. Clone this repository
2. Open the project in **Unity 2022.3 LTS**
3. Set up dependencies:
   - Vuforia Engine (via Unity Package Manager)
   - [MediaPipeUnityPlugin](https://github.com/homuler/MediaPipeUnityPlugin)
   - Google Cloud API credentials for Speech-to-Text & Text-to-Speech
   - Access Gemini API and Open-Meteo

## 📚 References

- [Unity3D](https://unity.com/)
- [Vuforia Engine](https://developer.vuforia.com/)
- [MediaPipe Unity Plugin](https://github.com/homuler/MediaPipeUnityPlugin)
- [Google Cloud Speech API](https://cloud.google.com/speech-to-text)
- [Open-Meteo Weather API](https://open-meteo.com/)
- [Blender](https://www.blender.org/)

## 👩‍💻 Contributors

- Jiayi Ouyang  
- Yuning Sun  
- Zishuo Wang  

> Trinity College Dublin, 2025

---

