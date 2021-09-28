<div align="center">
  <img src="https://github.com/DavidFernandezChaves/ViMantic-Unity3DNode/blob/master/Resources/Vimantic.gif?raw=true" alt="ViMantic" width="400" height="200"/>
</div>

# ViMantic Server (Unity)

ViMantic is a distributed architecture for semantic mapping of environments using mobile robots. For this, we have used Unity to create virtual environments that represent the information obtained from the real environment. This architecture is composed of one or several clients (robots/agents) and a server.

## Features
- Use an ontology as a formal and clear model to accommodate semantic information, including also mechanisms for its manipulation, i.e. insertion, modification or query.
- The model is automatically populated, i.e. it has a method to transform sensory data into high-level information, e.g. by recognizing objects.
- ViMantic uses 3D virtual maps to show the semantic knowledge acquired.

## Requirements
- [ViMantic - Client](https://github.com/DavidFernandezChaves/ViMantic-Client)

## Method of use:
Download and embed in your Unity project.

## Example
Result obtained using [Robot@VirtualHome](https://github.com/DavidFernandezChaves/RobotAtVirtualHome):
<div align="center">
  <img src="https://github.com/DavidFernandezChaves/ViMantic-Unity3DNode/blob/master/Resources/example.png?raw=true"/>
</div>

## Reference
If you use ViMantic in your research, use the following BibTeX entry.

```
@article{fernandez2021vimantic,
  title={ViMantic, a distributed robotic architecture for semantic mapping in indoor environments},
  author={Fernandez-Chaves, D and Ruiz-Sarmiento, JR and Petkov, N and Gonzalez-Jimenez, J},
  journal={Knowledge-Based Systems},
  pages={107440},
  year={2021},
  publisher={Elsevier}
}
