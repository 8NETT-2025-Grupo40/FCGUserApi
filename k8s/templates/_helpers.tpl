{{- define "fcg-user-api.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end }}

{{- define "fcg-user-api.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name (include "fcg-user-api.name" .) | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end }}

{{- define "fcg-user-api.labels" -}}
helm.sh/chart: {{ .Chart.Name }}-{{ .Chart.Version | replace "+" "_" }}
app.kubernetes.io/name: {{ include "fcg-user-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/version: {{ .Chart.AppVersion }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{- define "fcg-user-api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "fcg-user-api.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{- define "fcg-user-api.serviceAccountName" -}}
{{- if .Values.serviceAccount.name -}}
{{- .Values.serviceAccount.name -}}
{{- else -}}
{{- include "fcg-user-api.fullname" . -}}
{{- end -}}
{{- end }}
