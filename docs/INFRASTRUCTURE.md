# FCG User API - Infrastructure Dependencies & Deployment

This document describes the infrastructure requirements and deployment process for the FCG User API on Amazon EKS.

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Infrastructure Dependencies](#infrastructure-dependencies)
- [Deployment Process](#deployment-process)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)

## 🏗️ Architecture Overview

The FCG User API is deployed on a **shared EKS cluster** managed by the [`fcg-infra`](https://github.com/8NETT-2025-Grupo40/fcg-infra) repository. The architecture follows these principles:

- **Shared Cluster**: Multiple APIs share the same EKS cluster (`fcg`)
- **Isolated Deployments**: Each API has its own Kubernetes resources (Deployment, Service, etc.)
- **Shared ALB**: All APIs share a single Application Load Balancer with path-based routing
- **Namespace**: All FCG services deploy to the `fcg` namespace

```
┌─────────────────────────────────────────────────────────────┐
│                      AWS Account                            │
│  ┌───────────────────────────────────────────────────────┐  │
│  │              EKS Cluster: fcg                         │  │
│  │  ┌─────────────────────────────────────────────────┐  │  │
│  │  │  Namespace: fcg                                 │  │  │
│  │  │                                                 │  │  │
│  │  │  ┌──────────────┐  ┌──────────────┐           │  │  │
│  │  │  │  FCG User    │  │  FCG Order   │  ...      │  │  │
│  │  │  │  API Pods    │  │  API Pods    │           │  │  │
│  │  │  └──────────────┘  └──────────────┘           │  │  │
│  │  │                                                 │  │  │
│  │  │  ┌──────────────────────────────────────────┐  │  │  │
│  │  │  │  Shared ALB (path-based routing)        │  │  │  │
│  │  │  │  - /users/* → User API                  │  │  │  │
│  │  │  │  - /orders/* → Order API                │  │  │  │
│  │  │  └──────────────────────────────────────────┘  │  │  │
│  │  └─────────────────────────────────────────────────┘  │  │
│  │                                                         │  │
│  │  Add-ons:                                              │  │
│  │  - AWS Load Balancer Controller (kube-system)         │  │
│  │  - External Secrets Operator (external-secrets)       │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌───────────────┐  ┌──────────────┐  ┌────────────────┐   │
│  │  ECR Repo     │  │  Secrets     │  │  RDS Database  │   │
│  │  fcg-user-api │  │  Manager     │  │  (SQL Server)  │   │
│  └───────────────┘  └──────────────┘  └────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## ✅ Prerequisites

### 1. Shared Infrastructure (fcg-infra repository)

Before deploying this API, the following must be set up by the [`fcg-infra`](https://github.com/8NETT-2025-Grupo40/fcg-infra) repository:

- ✅ **EKS Cluster**: `fcg` cluster in `us-east-1`
- ✅ **Namespace**: `fcg` namespace created
- ✅ **AWS Load Balancer Controller**: Installed in `kube-system`
- ✅ **External Secrets Operator**: Installed in `external-secrets`
- ✅ **IAM Policy**: `FCGExternalSecretsPolicy` (ARN: `arn:aws:iam::478511033947:policy/FCGExternalSecretsPolicy`)

**To verify cluster readiness:**

```bash
# Check cluster exists
aws eks describe-cluster --name fcg --region us-east-1

# Check namespaces
kubectl get namespaces fcg external-secrets

# Check add-ons
kubectl get deployment -n kube-system aws-load-balancer-controller
kubectl get deployment -n external-secrets external-secrets
```

### 2. AWS Resources (Application-Specific)

The following resources must exist before deployment:

#### ECR Repository
```bash
# Create if doesn't exist
aws ecr create-repository \
  --repository-name fcg-user-api \
  --region us-east-1
```

#### AWS Secrets Manager Secrets

1. **RDS Connection String** (`fcg-api-user-connection-string`)
   ```json
   {
     "connectionString": "Server=<rds-endpoint>;Database=FCGUser;User Id=<user>;Password=<password>;TrustServerCertificate=True;"
   }
   ```

2. **JWT Configuration** (`fcg-jwt-config`)
   ```json
   {
     "issuer": "https://fcg-api.example.com",
     "audience": "fcg-users",
     "key": "<your-secret-key>"
   }
   ```

**Create secrets:**
```bash
# Connection string
aws secretsmanager create-secret \
  --name fcg-api-user-connection-string \
  --secret-string '{"connectionString":"<your-connection-string>"}' \
  --region us-east-1

# JWT config
aws secretsmanager create-secret \
  --name fcg-jwt-config \
  --secret-string '{"issuer":"<issuer>","audience":"<audience>","key":"<secret-key>"}' \
  --region us-east-1
```

### 3. GitHub Secrets

Configure in **Settings → Secrets and variables → Actions**:

- `AWS_ACCESS_KEY_ID`: AWS access key with permissions for:
  - ECR push/pull
  - EKS describe/update-kubeconfig
  - Secrets Manager read (for migrations)
  - IAM CreateServiceAccount (for IRSA)
  
- `AWS_SECRET_ACCESS_KEY`: Corresponding secret key

## 🔗 Infrastructure Dependencies

### Managed by fcg-infra Repository

| Resource | Type | Purpose | Managed By |
|----------|------|---------|------------|
| EKS Cluster `fcg` | AWS EKS | Kubernetes control plane | `fcg-infra` |
| Node Group `low-cost` | EC2 | Worker nodes (t3a.small, 2-3 nodes) | `fcg-infra` |
| AWS Load Balancer Controller | Helm Chart | Manages ALB for Ingresses | `fcg-infra` |
| External Secrets Operator | Helm Chart | Syncs secrets from AWS | `fcg-infra` |
| Namespace `fcg` | Kubernetes | Isolation for FCG services | `fcg-infra` |
| IAM Policy `FCGExternalSecretsPolicy` | AWS IAM | Secrets Manager access | `fcg-infra` |
| VPC `vpc-0e6d1df089da1ec39` | AWS VPC | Networking | `fcg-infra` |

### Managed by This Repository (FCGUserApi)

| Resource | Type | Purpose | Managed By |
|----------|------|---------|------------|
| ECR Repository `fcg-user-api` | AWS ECR | Docker image storage | This API |
| Docker Image | Docker | Application container | This API |
| ServiceAccount `fcg-user-api-sa` | Kubernetes + IRSA | AWS credentials for pods | This API (workflow) |
| Deployment `fcg-user-api` | Kubernetes | Pod management | This API (Helm) |
| Service `fcg-user-api` | Kubernetes | Internal networking | This API (Helm) |
| Ingress `fcg-user-api` | Kubernetes | ALB routing rules | This API (Helm) |
| ExternalSecret | Kubernetes | Secret sync config | This API (Helm) |
| SecretStore | Kubernetes | AWS connection config | This API (Helm) |
| HPA | Kubernetes | Autoscaling (2-5 pods) | This API (Helm) |

## 🚀 Deployment Process

### Automatic Deployment (GitHub Actions)

The deployment happens automatically on push to `main` or `kubernetes` branches:

1. **Build & Test** (`build` and `tests` jobs)
   - Restore dependencies
   - Build solution
   - Run unit tests
   - Generate test reports

2. **Build & Push Docker Image** (`build-push` job)
   - Build Docker image with tag `${{ github.sha }}`
   - Push to ECR with both SHA tag and `latest`
   - Example: `478511033947.dkr.ecr.us-east-1.amazonaws.com/fcg-user-api:abc123def`

3. **Deploy to EKS** (`deploy-eks` job)
   - Configure kubectl for EKS cluster
   - Create/update IRSA (ServiceAccount with IAM role)
   - Deploy via Helm with new image tag
   - Verify rollout status
   - Get ALB URL

### Manual Deployment (Local)

#### Prerequisites
```bash
# Install required tools
choco install kubernetes-cli kubernetes-helm eksctl awscli
```

#### Steps

1. **Configure AWS credentials**
   ```bash
   aws configure
   ```

2. **Update kubeconfig**
   ```bash
   aws eks update-kubeconfig --name fcg --region us-east-1
   ```

3. **Build and push Docker image**
   ```bash
   # Login to ECR
   aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin 478511033947.dkr.ecr.us-east-1.amazonaws.com
   
   # Build and push
   docker build -t 478511033947.dkr.ecr.us-east-1.amazonaws.com/fcg-user-api:latest .
   docker push 478511033947.dkr.ecr.us-east-1.amazonaws.com/fcg-user-api:latest
   ```

4. **Create/Update IRSA**
   ```bash
   eksctl create iamserviceaccount \
     --cluster=fcg \
     --namespace=fcg \
     --name=fcg-user-api-sa \
     --attach-policy-arn=arn:aws:iam::478511033947:policy/FCGExternalSecretsPolicy \
     --approve \
     --override-existing-serviceaccounts \
     --region=us-east-1
   ```

5. **Deploy with Helm**
   ```bash
   helm upgrade --install fcg-user-api ./k8s \
     --namespace fcg \
     --create-namespace \
     --set image.tag=latest \
     --wait \
     --timeout 5m
   ```

6. **Verify deployment**
   ```bash
   kubectl get pods -n fcg -l app.kubernetes.io/name=fcg-user-api
   kubectl get ingress -n fcg
   ```

7. **Get ALB URL**
   ```bash
   kubectl get ingress -n fcg fcg-user-api-fcg-user-api -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
   ```

## ⚙️ Configuration

### Helm Chart Values

The deployment is configured via `k8s/values.yaml`:

```yaml
image:
  repository: "478511033947.dkr.ecr.us-east-1.amazonaws.com/fcg-user-api"
  tag: "latest"  # Overridden in CI/CD with git SHA

replicaCount: 2  # Initial replicas

containerPort: 5067  # ASP.NET Core port

ingress:
  enabled: true
  className: alb
  annotations:
    alb.ingress.kubernetes.io/group.name: fcg  # Shares ALB with other APIs

autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 5
  cpu: 60  # Scale at 60% CPU
  memory: 70  # Scale at 70% memory

serviceAccount:
  create: false  # Created by eksctl via IRSA
  name: "fcg-user-api-sa"
```

### Environment Variables

Non-sensitive environment variables are set in `k8s/values.yaml`:

```yaml
env:
  ASPNETCORE_URLS: "http://+:5067"
  OTEL_EXPORTER_OTLP_ENDPOINT: "http://otel-collector:4317"
```

### Secrets (via External Secrets Operator)

Sensitive values are synced from AWS Secrets Manager:

```yaml
# k8s/templates/externalsecret.yaml
apiVersion: external-secrets.io/v1beta1
kind: ExternalSecret
metadata:
  name: fcg-user-api-secrets
spec:
  secretStoreRef:
    name: fcg-user-api-secret-store
  target:
    name: fcg-user-api-secrets
  data:
    - secretKey: ConnectionStrings__DefaultConnection
      remoteRef:
        key: fcg-api-user-connection-string
        property: connectionString
    - secretKey: Jwt__Issuer
      remoteRef:
        key: fcg-jwt-config
        property: issuer
    # ... etc
```

## 🐛 Troubleshooting

### Pods Not Starting

```bash
# Check pod status
kubectl get pods -n fcg -l app.kubernetes.io/name=fcg-user-api

# Describe pod for events
kubectl describe pod -n fcg <pod-name>

# Check logs
kubectl logs -n fcg <pod-name>
```

**Common issues:**
- **ImagePullBackOff**: ECR image doesn't exist or no permissions
  - Verify image exists: `aws ecr describe-images --repository-name fcg-user-api --region us-east-1`
- **CrashLoopBackOff**: Application crashes on startup
  - Check logs for connection string or configuration errors
- **Pending**: Insufficient cluster capacity
  - Check node status: `kubectl get nodes`

### Secrets Not Available

```bash
# Check ExternalSecret status
kubectl get externalsecret -n fcg fcg-user-api-secrets
kubectl describe externalsecret -n fcg fcg-user-api-secrets

# Check SecretStore
kubectl get secretstore -n fcg
kubectl describe secretstore -n fcg fcg-user-api-secret-store

# Check if secret was created
kubectl get secret -n fcg fcg-user-api-secrets
```

**Common issues:**
- **SecretSyncError**: Cannot read from Secrets Manager
  - Verify IRSA annotation: `kubectl get sa fcg-user-api-sa -n fcg -o yaml`
  - Check IAM policy allows reading the specific secret
  - Verify secret exists: `aws secretsmanager describe-secret --secret-id fcg-api-user-connection-string --region us-east-1`

### ALB Not Provisioning

```bash
# Check Ingress status
kubectl describe ingress -n fcg fcg-user-api-fcg-user-api

# Check ALB Controller logs
kubectl logs -n kube-system deployment/aws-load-balancer-controller

# List AWS ALBs
aws elbv2 describe-load-balancers --region us-east-1 --query "LoadBalancers[?contains(LoadBalancerName, 'k8s-fcg')]"
```

**Common issues:**
- **No LoadBalancer address**: ALB still provisioning (can take 2-5 minutes)
- **Error events**: Check controller logs for IAM permission issues
- **Target group unhealthy**: Check health endpoint `/health` is responding

### Deployment Fails in GitHub Actions

```bash
# Check workflow logs in GitHub Actions UI
```

**Common issues:**
- **ECR push failed**: Check AWS credentials in GitHub Secrets
- **kubectl command failed**: Cluster doesn't exist or wrong region
- **Helm timeout**: Pods taking too long to become ready (check pod logs)
- **IRSA creation failed**: IAM policy doesn't exist or no permissions

### Database Connection Issues

```bash
# Check connection string secret
kubectl get secret fcg-user-api-secrets -n fcg -o jsonpath='{.data.ConnectionStrings__DefaultConnection}' | base64 -d

# Test connectivity from pod
kubectl exec -it -n fcg <pod-name> -- /bin/sh
# Then try: curl <rds-endpoint>:1433 (SQL Server port)
```

**Common issues:**
- **Wrong connection string**: Update secret in AWS Secrets Manager
- **Network issues**: Check VPC security groups allow traffic from EKS to RDS
- **Authentication failed**: Verify SQL Server credentials

### Horizontal Pod Autoscaler Not Scaling

```bash
# Check HPA status
kubectl get hpa -n fcg fcg-user-api

# Describe for details
kubectl describe hpa -n fcg fcg-user-api

# Check metrics-server (if installed)
kubectl top pods -n fcg
```

**Common issues:**
- **Unknown metrics**: Metrics server not installed (optional)
- **Not scaling up**: CPU/memory thresholds not reached
- **At max replicas**: Increase `maxReplicas` in values.yaml

## 📚 Additional Resources

- [fcg-infra Repository](https://github.com/8NETT-2025-Grupo40/fcg-infra) - Shared infrastructure
- [AWS Load Balancer Controller](https://kubernetes-sigs.github.io/aws-load-balancer-controller/)
- [External Secrets Operator](https://external-secrets.io/)
- [Helm Documentation](https://helm.sh/docs/)
- [EKS Best Practices](https://aws.github.io/aws-eks-best-practices/)

## 🔄 Related Workflows

### When to Run Each Workflow

| Workflow | Repository | When to Run | Frequency |
|----------|-----------|-------------|-----------|
| **Setup Cluster** | `fcg-infra` | Initial setup or cluster recreation | Once |
| **Deploy API** | `FCGUserApi` | After code changes | Every push to main |
| **Destroy Cluster** | `fcg-infra` | Cost cleanup or major changes | Rarely |

### Typical Workflow Order

1. **Initial Setup** (once):
   ```
   fcg-infra: Setup Cluster → Deploy all APIs
   ```

2. **Daily Development** (per API):
   ```
   FCGUserApi: Push to main → Auto-deploy via GitHub Actions
   ```

3. **Adding New API**:
   ```
   1. Ensure cluster exists (fcg-infra)
   2. Create ECR repository for new API
   3. Create AWS Secrets for new API
   4. Deploy new API (similar to FCGUserApi)
   ```

## 🤝 Support

For issues related to:
- **This API's deployment**: Open issue in this repository
- **EKS cluster/add-ons**: Open issue in [`fcg-infra`](https://github.com/8NETT-2025-Grupo40/fcg-infra)
- **AWS resources**: Contact DevOps team
