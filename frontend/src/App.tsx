import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type ProjectStatus = 'Active' | 'Archived'
type EnvironmentType = 'Development' | 'Staging' | 'Production'
type EnvironmentStatus = 'Active' | 'Inactive'

type Project = {
  id: string
  name: string
  description: string | null
  environment: string
  version: string
  status: ProjectStatus | number | string
  createdAt: string
  updatedAt: string
}

type Environment = {
  id: string
  projectId: string
  name: string
  type: EnvironmentType | number | string
  url: string
  status: EnvironmentStatus | number | string
  createdAt: string
  updatedAt: string
}

const normalizeStatus = (status: ProjectStatus | number | string) => {
  if (typeof status === 'number') {
    return status === 0 ? 'Active' : 'Archived'
  }

  return status === 'Active' || status === 'Archived' ? status : 'Active'
}

const toStatusValue = (status: ProjectStatus) => (status === 'Active' ? 0 : 1)

const normalizeEnvironmentType = (type: Environment['type']): EnvironmentType => {
  if (typeof type === 'number') {
    return type === 1 ? 'Staging' : type === 2 ? 'Production' : 'Development'
  }

  return type === 'Staging' || type === 'Production' ? type : 'Development'
}

const normalizeEnvironmentStatus = (status: Environment['status']): EnvironmentStatus => {
  if (typeof status === 'number') {
    return status === 1 ? 'Inactive' : 'Active'
  }

  return status === 'Inactive' ? 'Inactive' : 'Active'
}

const toEnvironmentTypeValue = (type: EnvironmentType) =>
  type === 'Staging' ? 1 : type === 'Production' ? 2 : 0

const toEnvironmentStatusValue = (status: EnvironmentStatus) => (status === 'Active' ? 0 : 1)

type ProjectForm = {
  name: string
  description: string
  environment: string
  version: string
  status: ProjectStatus
}

const emptyForm: ProjectForm = {
  name: '',
  description: '',
  environment: 'Development',
  version: '1.0.0',
  status: 'Active',
}

type EnvironmentForm = {
  name: string
  type: EnvironmentType
  url: string
  status: EnvironmentStatus
}

const emptyEnvironmentForm: EnvironmentForm = {
  name: '',
  type: 'Development',
  url: '',
  status: 'Active',
}

function App() {
  const [projects, setProjects] = useState<Project[]>([])
  const [form, setForm] = useState<ProjectForm>(emptyForm)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [editingProjectId, setEditingProjectId] = useState<string | null>(null)
  const [selectedProjectId, setSelectedProjectId] = useState<string | null>(null)
  const [environments, setEnvironments] = useState<Environment[]>([])
  const [environmentForm, setEnvironmentForm] = useState<EnvironmentForm>(emptyEnvironmentForm)
  const [editingEnvironmentId, setEditingEnvironmentId] = useState<string | null>(null)
  const [environmentsLoading, setEnvironmentsLoading] = useState(false)
  const [environmentSubmitting, setEnvironmentSubmitting] = useState(false)

  const loadProjects = async () => {
    try {
      setLoading(true)
      const response = await fetch('/api/projects')

      if (!response.ok) {
        throw new Error('No se pudieron cargar los proyectos.')
      }

      const data = (await response.json()) as Project[]
      setProjects(data)
      setError('')
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Error inesperado.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadProjects()
  }, [])

  const loadEnvironments = async (projectId: string) => {
    try {
      setEnvironmentsLoading(true)
      const response = await fetch(`/api/projects/${projectId}/environments`)

      if (!response.ok) {
        throw new Error('No se pudieron cargar los environments.')
      }

      setEnvironments((await response.json()) as Environment[])
      setError('')
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Error inesperado.')
    } finally {
      setEnvironmentsLoading(false)
    }
  }

  const resetForm = () => {
    setForm(emptyForm)
    setEditingProjectId(null)
  }

  const resetEnvironmentForm = () => {
    setEnvironmentForm(emptyEnvironmentForm)
    setEditingEnvironmentId(null)
  }

  const handleManageEnvironments = (projectId: string) => {
    setSelectedProjectId(projectId)
    resetEnvironmentForm()
    void loadEnvironments(projectId)
  }

  const handleEdit = (project: Project) => {
    setEditingProjectId(project.id)
    setForm({
      name: project.name,
      description: project.description ?? '',
      environment: project.environment,
      version: project.version,
      status: normalizeStatus(project.status) as ProjectStatus,
    })
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    if (!form.name.trim() || !form.environment.trim() || !form.version.trim()) {
      setError('Nombre, entorno y versión son obligatorios.')
      return
    }

    try {
      setSubmitting(true)
      setError('')

      const url = editingProjectId ? `/api/projects/${editingProjectId}` : '/api/projects'
      const method = editingProjectId ? 'PUT' : 'POST'

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: form.name,
          description: form.description || null,
          environment: form.environment,
          version: form.version,
          status: toStatusValue(form.status),
        }),
      })

      if (!response.ok) {
        throw new Error(editingProjectId ? 'No se pudo actualizar el proyecto.' : 'No se pudo crear el proyecto.')
      }

      resetForm()
      await loadProjects()
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Error al guardar.')
    } finally {
      setSubmitting(false)
    }
  }

  const handleEnvironmentEdit = (environment: Environment) => {
    setEditingEnvironmentId(environment.id)
    setEnvironmentForm({
      name: environment.name,
      type: normalizeEnvironmentType(environment.type),
      url: environment.url,
      status: normalizeEnvironmentStatus(environment.status),
    })
  }

  const handleEnvironmentSubmit = async (event: FormEvent) => {
    event.preventDefault()

    if (!selectedProjectId || !environmentForm.name.trim() || !environmentForm.url.trim()) {
      setError('Nombre y URL del environment son obligatorios.')
      return
    }

    try {
      setEnvironmentSubmitting(true)
      setError('')

      const url = editingEnvironmentId
        ? `/api/projects/${selectedProjectId}/environments/${editingEnvironmentId}`
        : `/api/projects/${selectedProjectId}/environments`
      const method = editingEnvironmentId ? 'PUT' : 'POST'
      const body = {
        name: environmentForm.name,
        type: toEnvironmentTypeValue(environmentForm.type),
        url: environmentForm.url,
        ...(editingEnvironmentId ? { status: toEnvironmentStatusValue(environmentForm.status) } : {}),
      }

      const response = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body),
      })

      if (!response.ok) {
        throw new Error(editingEnvironmentId ? 'No se pudo actualizar el environment.' : 'No se pudo crear el environment.')
      }

      resetEnvironmentForm()
      await loadEnvironments(selectedProjectId)
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Error al guardar el environment.')
    } finally {
      setEnvironmentSubmitting(false)
    }
  }

  const handleEnvironmentDelete = async (environmentId: string) => {
    if (!selectedProjectId) {
      return
    }

    try {
      const response = await fetch(`/api/projects/${selectedProjectId}/environments/${environmentId}`, {
        method: 'DELETE',
      })

      if (!response.ok) {
        throw new Error('No se pudo eliminar el environment.')
      }

      await loadEnvironments(selectedProjectId)
    } catch (deleteError) {
      setError(deleteError instanceof Error ? deleteError.message : 'Error al eliminar el environment.')
    }
  }

  const handleDelete = async (id: string) => {
    try {
      const response = await fetch(`/api/projects/${id}`, {
        method: 'DELETE',
      })

      if (!response.ok) {
        throw new Error('No se pudo eliminar el proyecto.')
      }

      await loadProjects()
      if (selectedProjectId === id) {
        setSelectedProjectId(null)
        setEnvironments([])
      }
    } catch (deleteError) {
      setError(deleteError instanceof Error ? deleteError.message : 'Error al eliminar.')
    }
  }

  return (
    <main className="page-shell">
      <section className="panel">
        <div className="panel-header">
          <div>
            <p className="eyebrow">QOps</p>
            <h1>Projects</h1>
          </div>
          <button className="ghost-button" type="button" onClick={() => void loadProjects()}>
            Refresh
          </button>
        </div>

        <form className="project-form" onSubmit={handleSubmit}>
          <label>
            <span>Name</span>
            <input
              value={form.name}
              onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
              placeholder="QOps API"
            />
          </label>

          <label>
            <span>Description</span>
            <textarea
              value={form.description}
              onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
              placeholder="Project details"
              rows={3}
            />
          </label>

          <div className="form-grid">
            <label>
              <span>Environment</span>
              <input
                value={form.environment}
                onChange={(event) => setForm((current) => ({ ...current, environment: event.target.value }))}
                placeholder="Development"
              />
            </label>

            <label>
              <span>Version</span>
              <input
                value={form.version}
                onChange={(event) => setForm((current) => ({ ...current, version: event.target.value }))}
                placeholder="1.0.0"
              />
            </label>

            <label>
              <span>Status</span>
              <select
                value={form.status}
                onChange={(event) =>
                  setForm((current) => ({ ...current, status: event.target.value as ProjectStatus }))
                }
              >
                <option value="Active">Active</option>
                <option value="Archived">Archived</option>
              </select>
            </label>
          </div>

          {error ? <p className="form-error">{error}</p> : null}

          <div className="action-row">
            <button className="primary-button" type="submit" disabled={submitting}>
              {submitting ? 'Saving...' : editingProjectId ? 'Update project' : 'Create project'}
            </button>

            {editingProjectId ? (
              <button className="ghost-button" type="button" onClick={resetForm}>
                Cancel
              </button>
            ) : null}
          </div>
        </form>
      </section>

      <section className="panel">
        <div className="table-header">
          <h2>Project list</h2>
          <span>{projects.length} items</span>
        </div>

        {loading ? (
          <p className="empty-state">Loading projects...</p>
        ) : projects.length === 0 ? (
          <p className="empty-state">No projects yet.</p>
        ) : (
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Environment</th>
                  <th>Version</th>
                  <th>Status</th>
                  <th>Updated</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {projects.map((project) => {
                  const statusLabel = normalizeStatus(project.status)

                  return (
                    <tr key={project.id}>
                      <td>
                        <div className="project-name-cell">
                          <strong>{project.name}</strong>
                          {project.description ? <small>{project.description}</small> : null}
                        </div>
                      </td>
                      <td>{project.environment}</td>
                      <td>{project.version}</td>
                      <td>
                        <span className={`status-badge ${statusLabel.toLowerCase()}`}>
                          {statusLabel}
                        </span>
                      </td>
                      <td>{new Date(project.updatedAt).toLocaleDateString()}</td>
                      <td>
                        <div className="row-actions">
                          <button className="edit-button" type="button" onClick={() => handleEdit(project)}>
                            Edit
                          </button>
                          <button
                            className="environment-button"
                            type="button"
                            onClick={() => handleManageEnvironments(project.id)}
                          >
                            Environments
                          </button>
                          <button className="delete-button" type="button" onClick={() => void handleDelete(project.id)}>
                            Delete
                          </button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {selectedProjectId ? (
        <section className="panel environment-panel">
          <div className="panel-header">
            <div>
              <p className="eyebrow">Project environments</p>
              <h2>{projects.find((project) => project.id === selectedProjectId)?.name ?? 'Selected project'}</h2>
            </div>
            <div className="action-row">
              <button className="ghost-button" type="button" onClick={() => void loadEnvironments(selectedProjectId)}>
                Refresh
              </button>
              <button className="ghost-button" type="button" onClick={() => setSelectedProjectId(null)}>
                Close
              </button>
            </div>
          </div>

          <form className="project-form" onSubmit={handleEnvironmentSubmit}>
            <div className="form-grid environment-form-grid">
              <label>
                <span>Name</span>
                <input
                  value={environmentForm.name}
                  onChange={(event) => setEnvironmentForm((current) => ({ ...current, name: event.target.value }))}
                  placeholder="Development"
                />
              </label>

              <label>
                <span>Type</span>
                <select
                  value={environmentForm.type}
                  onChange={(event) =>
                    setEnvironmentForm((current) => ({ ...current, type: event.target.value as EnvironmentType }))
                  }
                >
                  <option value="Development">Development</option>
                  <option value="Staging">Staging</option>
                  <option value="Production">Production</option>
                </select>
              </label>

              <label>
                <span>URL</span>
                <input
                  type="url"
                  value={environmentForm.url}
                  onChange={(event) => setEnvironmentForm((current) => ({ ...current, url: event.target.value }))}
                  placeholder="https://dev.example.com"
                />
              </label>

              {editingEnvironmentId ? (
                <label>
                  <span>Status</span>
                  <select
                    value={environmentForm.status}
                    onChange={(event) =>
                      setEnvironmentForm((current) => ({
                        ...current,
                        status: event.target.value as EnvironmentStatus,
                      }))
                    }
                  >
                    <option value="Active">Active</option>
                    <option value="Inactive">Inactive</option>
                  </select>
                </label>
              ) : null}
            </div>

            <div className="action-row">
              <button className="primary-button" type="submit" disabled={environmentSubmitting}>
                {environmentSubmitting ? 'Saving...' : editingEnvironmentId ? 'Update environment' : 'Create environment'}
              </button>
              {editingEnvironmentId ? (
                <button className="ghost-button" type="button" onClick={resetEnvironmentForm}>
                  Cancel
                </button>
              ) : null}
            </div>
          </form>

          {environmentsLoading ? (
            <p className="empty-state">Loading environments...</p>
          ) : environments.length === 0 ? (
            <p className="empty-state">No environments yet.</p>
          ) : (
            <div className="table-wrapper environment-table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Type</th>
                    <th>URL</th>
                    <th>Status</th>
                    <th>Updated</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {environments.map((environment) => {
                    const typeLabel = normalizeEnvironmentType(environment.type)
                    const statusLabel = normalizeEnvironmentStatus(environment.status)

                    return (
                      <tr key={environment.id}>
                        <td><strong>{environment.name}</strong></td>
                        <td>{typeLabel}</td>
                        <td><a href={environment.url} target="_blank" rel="noreferrer">{environment.url}</a></td>
                        <td><span className={`status-badge ${statusLabel.toLowerCase()}`}>{statusLabel}</span></td>
                        <td>{new Date(environment.updatedAt).toLocaleDateString()}</td>
                        <td>
                          <div className="row-actions">
                            <button className="edit-button" type="button" onClick={() => handleEnvironmentEdit(environment)}>
                              Edit
                            </button>
                            <button className="delete-button" type="button" onClick={() => void handleEnvironmentDelete(environment.id)}>
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          )}
        </section>
      ) : null}
    </main>
  )
}

export default App
