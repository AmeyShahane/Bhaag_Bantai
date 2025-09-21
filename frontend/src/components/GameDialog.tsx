import React from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
  TextField,
} from "@mui/material";

type GameDialogProps = {
  open: boolean;
  mode: "new" | "join";
  character: string;
  setCharacter: (c: "Bantai" | "Pandu") => void;
  onClose: () => void;
  onSubmit: (formValue: string, character: string) => void;
  error?: string | null;
};

export const GameDialog: React.FC<GameDialogProps> = ({
  open,
  mode,
  character,
  setCharacter,
  onClose,
  onSubmit,
  error,
}) => {
  return (
    <Dialog
      open={open}
      onClose={onClose}
      slotProps={{
        paper: {
          component: "form",
          onSubmit: (event: React.FormEvent<HTMLFormElement>) => {
            event.preventDefault();
            const formData = new FormData(event.currentTarget);
            const formJson = Object.fromEntries((formData as any).entries());
            const fieldName = mode === "new" ? "playerName" : "roomCode";
            const value = formJson[fieldName];
            onClose();
            onSubmit(value as string, character);
          },
        },
      }}
    >
      <DialogTitle>{mode === "new" ? "New Game" : "Join Game"}</DialogTitle>

      <DialogContent>
        {mode === "new" ? (
          <DialogContentText>
            {character === "Bantai"
              ? "Enter Your name (You will be Bantai, Bhagoooo Bhenchoood!!!)"
              : "Enter Your name (You will be Pandu, Gaand Marenge Bhenchod...)"}
          </DialogContentText>
        ) : (
          <DialogContentText>Enter your room code</DialogContentText>
        )}

        <TextField
          autoFocus
          required
          margin="dense"
          id={mode === "new" ? "playerName" : "roomCode"}
          name={mode === "new" ? "playerName" : "roomCode"}
          label={mode === "new" ? "Player Name" : "Room Code"}
          type="text"
          fullWidth
          variant="standard"
          slotProps={{
            input: {
              inputProps: {
                pattern: mode === "new" ? "^[A-Za-z_]+$" : "^[A-Za-z0-9]+$",
                title:
                  mode === "new"
                    ? "Only letters and underscores allowed"
                    : "Only letters and numbers allowed",
              },
            },
          }}
          error={!!error}
          helperText={error || ""}
        />

        {/* Character buttons shown in both cases */}
        <div style={{ marginTop: "1rem", display: "flex", gap: "1rem" }}>
          <Button
            variant={character === "Bantai" ? "contained" : "outlined"}
            onClick={() => setCharacter("Bantai")}
          >
            Play as Bantai
          </Button>
          <Button
            variant={character === "Pandu" ? "contained" : "outlined"}
            onClick={() => setCharacter("Pandu")}
          >
            Play as Pandu
          </Button>
        </div>
      </DialogContent>

      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button type="submit">Done</Button>
      </DialogActions>
    </Dialog>
  );
};
