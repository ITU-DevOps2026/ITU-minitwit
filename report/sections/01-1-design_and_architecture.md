## Design and Architecture

### Module view

![Package diagram of the minitwit application \label(fig:minitwit_package)](./images/minitwit_package_diagram.png){width=80%}

![Package diagram of the Model package in the minitwit application \label{fig:model_package}](./images/Model_package_diagram.png){width=80%}

### Component and Connectors (C&C) view

![C&C diagram of the minitwit application \label{fig:C&C}](./images/component_and_connector_diagram_horizontal.png)


### Allocation view

![Allocation View](./images/allocation_view.png)

### Sequence diagram of our system

In Figure \ref{fig:Sequence_diagram_user}, we show how our system handles a post message request from a user, via the rendered Razor page. 

In Figure \ref{fig:Sequence_diagram_api}, we show how our system handles a post message request, via the API. 

![Sequence Diagram of a user posting a message \label{fig:Sequence_diagram_user}](./images/interactions_user.png)

![Sequence Diagram of an API posting a message \label{fig:Sequence_diagram_api}](./images/interactions_api.png)
