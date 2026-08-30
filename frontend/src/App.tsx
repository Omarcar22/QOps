import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type ProjectStatus = 'Active' | 'Archived'

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

const normalizeStatus = (status: ProjectStatus | number | string) => {
  if (typeof status === 'number') {
    return status === 0 ? 'Active' : 'Archived'
  }

  return status === 'Active' || status === 'Archived' ? status : 'Active'
}

type ProjectForm = {
  name: string
  description: string
  environment: string
  version: string
}

const emptyForm: ProjectForm = {
  name: '',
  description: '',
  environment: 'Development',
  version: '1.0.0',
}

function App() {
  const [projects, setProjects] = useState<Project[]>([])
  const [form, setForm] = useState<ProjectForm>(emptyForm)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

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

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()

    if (!form.name.trim() || !form.environment.trim() || !form.version.trim()) {
      setError('Nombre, entorno y versión son obligatorios.')
      return
    }

    try {
      setSubmitting(true)
      setError('')

      const response = await fetch('/api/projects', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: form.name,
          description: form.description || null,
          environment: form.environment,
          version: form.version,
        }),
      })

      if (!response.ok) {
        throw new Error('No se pudo crear el proyecto.')
      }

      setForm(emptyForm)
      await loadProjects()
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Error al guardar.')
    } finally {
      setSubmitting(false)
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
          </div>

          {error ? <p className="form-error">{error}</p> : null}

          <button className="primary-button" type="submit" disabled={submitting}>
            {submitting ? 'Saving...' : 'Create project'}
          </button>
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
                        <button className="delete-button" type="button" onClick={() => void handleDelete(project.id)}>
                          Delete
                        </button>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </main>
  )
}

export default App
