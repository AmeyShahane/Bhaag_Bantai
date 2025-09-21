import { useState } from "react";
import { GameDialog } from "./GameDialog";
import { useNavigate } from "react-router-dom";

const Home = () => {
  const [showNewGameDialog, setShowNewGameDialog] = useState(false);
  const [showJoinGameDialog, setShowJoinGameDialog] = useState(false);
  const [character, setCharacter] = useState<"Bantai" | "Pandu">("Bantai");
  const [createError, setCreateError] = useState<string | null>(null);
  const [joinError, setJoinError] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleCreateGame = async (name: string, character: string) => {
    try {
      setCreateError(null);
      console.log("calling", process.env.REACT_APP_BACKEND_NEW_GAME_URL);
      const response = await fetch(
        process.env.REACT_APP_BACKEND_NEW_GAME_URL!,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ name, character }),
        }
      );

      if (!response.ok) {
        const err = await response.json();
        throw new Error(err.message || "Failed to create game");
      }

      const data = await response.json();
      navigate(`/lobby/${data.roomId}`, { state: { gameData: data } });
      setShowNewGameDialog(false);
    } catch (error: any) {
      setCreateError(error.message);
    }
  };

  // handle joining a game
  const handleJoinGame = async (roomCode: string, character: string) => {
    try {
      setJoinError(null);
      const response = await fetch(
        process.env.REACT_APP_BACKEND_JOIN_GAME_URL!,
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ roomCode, character }),
        }
      );

      if (!response.ok) {
        const err = await response.json();
        throw new Error(err.message || "Failed to join game");
      }

      const data = await response.json();
      navigate(`/lobby/${roomCode}`, { state: { gameData: data } });
      setShowJoinGameDialog(false);
    } catch (error: any) {
      setJoinError(error.message);
    }
  };

  return (
    <>
      <h1 className="text-4xl font-bold mb-8 text-gray-800">
        Welcome to Bhaag Bantai
      </h1>
      <div className="space-x-4">
        <button
          className="px-6 py-3 bg-blue-600 text-white rounded-xl shadow hover:bg-blue-700 transition"
          onClick={() => setShowJoinGameDialog(true)}
        >
          Join Game
        </button>

        <button
          className="px-6 py-3 bg-green-600 text-white rounded-xl shadow hover:bg-green-700 transition"
          onClick={() => setShowNewGameDialog(true)}
        >
          Create a New Game
        </button>
      </div>

      {/* New Game Dialog */}
      <GameDialog
        open={showNewGameDialog}
        mode="new"
        character={character}
        setCharacter={setCharacter}
        onClose={() => {
          setShowNewGameDialog(false);
          setCreateError(null);
        }}
        onSubmit={handleCreateGame}
        error={createError}
      />

      {/* Join Game Dialog */}
      <GameDialog
        open={showJoinGameDialog}
        mode="join"
        character={character}
        setCharacter={setCharacter}
        onClose={() => {
          setShowJoinGameDialog(false);
          setJoinError(null);
        }}
        onSubmit={handleJoinGame}
        error={joinError}
      />
    </>
  );
};

export default Home;
