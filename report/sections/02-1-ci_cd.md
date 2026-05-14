## CI/CD
- A complete description and illustration of stages and tools included in the CI/CD pipelines, including deployment and release of your systems.

For our CI/CD pipelines we decided to use GitHub Actions, since our codebase is hosted on GitHub and it then seemed like the simplest option to get started, compared to using external pipeline services. We have created several workflows that run based on different event triggers. As mentioned in section 1.1, we have both a production environment and an identical test environment; both of them are used in our CI/CD pipelines.  

Our CI pipeline includes both code analysis and automated testing. When a pull request is raised for a merge to main, a Codacy Static Code Analysis is run to analyze code quality and security issues. At the same time, the following workflows are triggered: a SonarQube Cloud static code analysis, a CodeQL code analysis to scan for security vulnerabilities and a Trivy security scan for Docker image vulnerabilities. There is also a workflow which runs automated tests, builds and pushes a new image of the MiniTwit application to DockerHub, and if all tests pass, this image is deployed to our test environment. This allows us to manually verify that the new changes from the PR integrate well into our existing application, and lowers the likelihood of unexpected issues when these changes are deployed to production. 

Our CD pipeline starts running when a push to main happens, e.g. when a PR is merged. This pipeline includes the same steps as our CI pipeline explained above, except that the final step after passing the automated tests is the new image of MiniTwit being deployed to the production environment instead of test environment. This means our system is continuously deployed to production whenever changes have been reviewed and pass several checks. 

FORKORT DET OVENOVER, EVT SAML DET TIL ET KORT AFSNIT OG UDFYLD MERE I SIMPLIFIED FIGUREN SÅDAN SÅ DU IK BEHØVER FORKLARE SÅ MEGET I TEKSTEN

In figure XX, a simplified diagram of the CI/CD pipeline is shown. Only the names of the stages are shown, the steps executed in each workflow are not included here. 

![Simplified CI/CD pipeline with no individual steps shown](./images/overall_cicd_pipeline.png){width=70%}

In figure XX is an illustration of the automated testing workflow. It was created as a reusable workflow, so that it can be reused in both the workflow for deployment to test environment and the one for deployment to production environment. 

![Workflow running automated tests](./images/test_pipeline.png){width=70%}

Figure XX illustrates the workflow that deploys to production environment. As mentioned before, it uses the workflow for tests shown in figure XX. 

![Workflow testing and deploying to production](./images/prod_cd_pipeline.png)

The workflow for deploying to test environment is very similar to figure XX, an illustration for it is included in appendix 5.1. Illustrations of the workflows for CodeQL, SonarQube and Trivy can be found in the same place. 

We also have a workflow for creating a draft for a new release. It is triggered whenever a new versioning tag is pushed. We would have liked to improve this workflow by finding some way to automate more of it, since it right now requires manual interference both for creating and pushing a tag, and then reviewing and publishing the release. This workflow is illustrated in appendix 5.1.

For writing and generating our report, we have set up a workflow with Pandoc which generates a new PDF for the report on every PR and push to main where changes have been made in the /report folder of our repository. This workflow is also illustrated in appendix 5.1. 