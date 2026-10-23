// Todo objeto con el que se puede interactuar con la tecla E implementa esta interfaz.
public interface IInteractable
{
    string Prompt { get; }   // texto que se muestra al apuntar al objeto
    void Interact();
}
