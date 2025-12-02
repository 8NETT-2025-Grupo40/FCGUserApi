# FCG User API - Kubernetes Deployment

This directory contains the Helm chart for deploying the FCG User API to Kubernetes (Amazon EKS).

## 📁 Structure

```
k8s/
├── Chart.yaml                 # Helm chart metadata
├── values.yaml                # Configuration values
└── templates/
    ├── _helpers.tpl          # Template helper functions
    ├── deployment.yaml       # Kubernetes Deployment
    ├── service.yaml          # Kubernetes Service (ClusterIP)
    ├── ingress.yaml          # ALB Ingress
    ├── hpa.yaml              # Horizontal Pod Autoscaler
    ├── externalsecret.yaml   # External Secrets sync config
    ├── secretstore.yaml      # AWS Secrets Manager connection
    └── serviceaccount.yaml   # ServiceAccount (IRSA)
```

## 🚀 Deployment

### Prerequisites

1. **EKS Cluster**: `fcg` cluster must exist (managed by `fcg-infra` repository)
2. **Namespace**: `fcg` namespace created
3. **Add-ons**: AWS Load Balancer Controller and External Secrets Operator installed
4. **ECR Image**: Docker image pushed to ECR
5. **AWS Secrets**: Connection string and JWT config in AWS Secrets Manager

### Deploy with Helm

```bash
# Update kubeconfig
aws eks update-kubeconfig --name fcg --region us-east-1

# Deploy
helm upgrade --install fcg-user-api ./k8s \
  --namespace fcg \
  --create-namespace \
  --set image.tag=latest \
  --wait

# Check status
kubectl get pods -n fcg -l app.kubernetes.io/name=fcg-user-api
kubectl get ingress -n fcg
```

### Override Values

```bash
# Use custom values file
helm upgrade --install fcg-user-api ./k8s \
  --namespace fcg \
  -f custom-values.yaml

# Override specific values
helm upgrade --install fcg-user-api ./k8s \
  --namespace fcg \
  --set image.tag=v1.2.3 \
  --set replicaCount=3 \
  --set autoscaling.maxReplicas=10
```

## ⚙️ Configuration

### Key Values

| Value | Default | Description |
|-------|---------|-------------|
| `image.repository` | `478511033947.dkr.ecr.us-east-1.amazonaws.com/fcg-user-api` | ECR repository |
| `image.tag` | `latest` | Image tag (overridden by CI/CD with git SHA) |
| `replicaCount` | `2` | Initial number of pods |
| `containerPort` | `5067` | ASP.NET Core listening port |
| `service.type` | `ClusterIP` | Kubernetes service type |
| `service.port` | `80` | Service port |
| `ingress.enabled` | `true` | Enable ALB ingress |
| `ingress.className` | `alb` | Ingress controller class |
| `autoscaling.enabled` | `true` | Enable HPA |
| `autoscaling.minReplicas` | `2` | Minimum pods |
| `autoscaling.maxReplicas` | `5` | Maximum pods |
| `serviceAccount.create` | `false` | ServiceAccount created by eksctl (IRSA) |
| `serviceAccount.name` | `fcg-user-api-sa` | ServiceAccount name |

### Environment Variables

Non-sensitive environment variables are configured in `values.yaml`:

```yaml
env:
  ASPNETCORE_URLS: "http://+:5067"
  OTEL_EXPORTER_OTLP_ENDPOINT: "http://otel-collector:4317"
```

### Secrets

Secrets are automatically synced from AWS Secrets Manager via External Secrets Operator:

- **Connection String**: `fcg-api-user-connection-string`
- **JWT Config**: `fcg-jwt-config`

The `ExternalSecret` resource watches these secrets and creates a Kubernetes Secret named `fcg-user-api-secrets`.

## 🔒 Security

### IRSA (IAM Roles for Service Accounts)

The deployment uses IRSA to grant pods AWS permissions without storing credentials:

1. **ServiceAccount**: `fcg-user-api-sa` (in `fcg` namespace)
2. **IAM Role**: Auto-created by eksctl with OIDC trust
3. **IAM Policy**: `FCGExternalSecretsPolicy` (allows reading secrets)

**Created by CI/CD workflow:**
```bash
eksctl create iamserviceaccount \
  --cluster=fcg \
  --namespace=fcg \
  --name=fcg-user-api-sa \
  --attach-policy-arn=arn:aws:iam::478511033947:policy/FCGExternalSecretsPolicy \
  --approve \
  --override-existing-serviceaccounts
```

### Resource Limits

```yaml
resources:
  requests:
    cpu: 100m      # Minimum CPU
    memory: 128Mi  # Minimum memory
  limits:
    cpu: 500m      # Maximum CPU
    memory: 512Mi  # Maximum memory
```

## 🌐 Networking

### Service

- **Type**: ClusterIP (internal only)
- **Port**: 80
- **Target Port**: 5067 (container port)

### Ingress (ALB)

- **Class**: `alb`
- **Scheme**: `internet-facing`
- **Target Type**: `ip`
- **Group**: `fcg` (shared with other APIs)
- **Health Check**: `/health`
- **Protocol**: HTTP (port 80)

**Annotations:**
```yaml
alb.ingress.kubernetes.io/scheme: internet-facing
alb.ingress.kubernetes.io/target-type: ip
alb.ingress.kubernetes.io/group.name: fcg  # Shares ALB with other services
alb.ingress.kubernetes.io/healthcheck-path: /health
```

### Path-Based Routing

Multiple APIs can share the same ALB by using the `group.name` annotation. Each API defines its own path:

```yaml
# User API
path: /

# Future: Order API could use
path: /orders
```

## 📊 Autoscaling

### Horizontal Pod Autoscaler (HPA)

Automatically scales pods based on CPU and memory utilization:

```yaml
autoscaling:
  enabled: true
  minReplicas: 2    # Minimum pods (high availability)
  maxReplicas: 5    # Maximum pods (cost control)
  cpu: 60          # Scale up at 60% CPU
  memory: 70       # Scale up at 70% memory
```

**Scaling behavior:**
- Starts with 2 pods
- Scales up when CPU > 60% OR memory > 70%
- Scales down when below thresholds (with cooldown)
- Never goes below 2 or above 5 pods

## 🔍 Monitoring

### Health Checks

```bash
# Get ALB URL
ALB_URL=$(kubectl get ingress -n fcg fcg-user-api-fcg-user-api -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')

# Test health endpoint
curl http://$ALB_URL/health
```

### Pod Status

```bash
# List pods
kubectl get pods -n fcg -l app.kubernetes.io/name=fcg-user-api

# Watch pod status
kubectl get pods -n fcg -l app.kubernetes.io/name=fcg-user-api -w

# Describe pod
kubectl describe pod -n fcg <pod-name>

# View logs
kubectl logs -n fcg <pod-name>

# Follow logs
kubectl logs -n fcg <pod-name> -f
```

### Resource Usage

```bash
# Pod metrics (requires metrics-server)
kubectl top pods -n fcg -l app.kubernetes.io/name=fcg-user-api

# HPA status
kubectl get hpa -n fcg fcg-user-api
kubectl describe hpa -n fcg fcg-user-api
```

### External Secrets Status

```bash
# Check ExternalSecret
kubectl get externalsecret -n fcg fcg-user-api-secrets
kubectl describe externalsecret -n fcg fcg-user-api-secrets

# Check if Kubernetes Secret was created
kubectl get secret -n fcg fcg-user-api-secrets

# Verify secret data (base64 encoded)
kubectl get secret -n fcg fcg-user-api-secrets -o yaml
```

## 🧹 Cleanup

### Uninstall Helm Release

```bash
helm uninstall fcg-user-api -n fcg
```

This removes:
- Deployment
- Service
- Ingress (and associated ALB)
- HPA
- ExternalSecret
- SecretStore

**Note**: Does NOT remove:
- ServiceAccount (managed by eksctl)
- ECR images
- AWS Secrets Manager secrets
- EKS cluster

### Remove ServiceAccount (IRSA)

```bash
eksctl delete iamserviceaccount \
  --cluster=fcg \
  --namespace=fcg \
  --name=fcg-user-api-sa \
  --region=us-east-1
```

## 🔧 Troubleshooting

### Helm Installation Failed

```bash
# Get Helm release status
helm status fcg-user-api -n fcg

# View Helm history
helm history fcg-user-api -n fcg

# Rollback to previous version
helm rollback fcg-user-api -n fcg
```

### Template Rendering Issues

```bash
# Dry-run to see rendered templates
helm install fcg-user-api ./k8s \
  --namespace fcg \
  --dry-run \
  --debug

# Render templates without installing
helm template fcg-user-api ./k8s \
  --namespace fcg \
  --set image.tag=test
```

### Update Values After Deployment

```bash
# Modify values.yaml, then upgrade
helm upgrade fcg-user-api ./k8s \
  --namespace fcg \
  --wait

# Force recreation of pods
helm upgrade fcg-user-api ./k8s \
  --namespace fcg \
  --recreate-pods \
  --wait
```

## 📚 References

- [Helm Documentation](https://helm.sh/docs/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [AWS Load Balancer Controller](https://kubernetes-sigs.github.io/aws-load-balancer-controller/)
- [External Secrets Operator](https://external-secrets.io/)
- [Infrastructure Documentation](../docs/INFRASTRUCTURE.md)
