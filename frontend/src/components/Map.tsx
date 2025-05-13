import React, { useEffect, useRef, useState } from 'react';

const Map: React.FC = () => {
  const [svgContent, setSvgContent] = useState<string>('');
  const containerRef = useRef<HTMLDivElement>(null);
  const svgWrapperRef = useRef<HTMLDivElement>(null);
  const svgRef = useRef<SVGSVGElement | null>(null);

  const [zoom, setZoom] = useState(1);
  const [position, setPosition] = useState({ x: 0, y: 0 });
  const [dragging, setDragging] = useState(false);
  const dragStart = useRef({ x: 0, y: 0 });
  const dragOffset = useRef({ x: 0, y: 0 });
  const [isInitialized, setIsInitialized] = useState(false);

  useEffect(() => {
    fetch('http://localhost:5221/api/game/map-svg')
      .then((res) => res.text())
      .then((data) => setSvgContent(data))
      .catch((err) => console.error('Error fetching SVG:', err));
  }, []);

  // Set zoom to fit screen after SVG loads
  useEffect(() => {
    if (!svgContent) return;

    const timer = setTimeout(() => {
      const svgElement = svgWrapperRef.current?.querySelector('svg');
      if (svgElement && containerRef.current) {
        svgRef.current = svgElement as SVGSVGElement;

        const viewBox = svgRef.current.viewBox.baseVal;
        const containerRect = containerRef.current.getBoundingClientRect();

        const scaleX = containerRect.width / viewBox.width;
        const scaleY = containerRect.height / viewBox.height;
        const fitZoom = Math.min(scaleX, scaleY);

        setZoom(fitZoom + 5);
        setPosition({ x: 0, y: 0 });
        setIsInitialized(true);
      }
    }, 100); // Delay to ensure DOM is updated

    return () => clearTimeout(timer);
  }, [svgContent]);

  const handleWheel = (e: React.WheelEvent) => {
    e.preventDefault();
    const delta = e.deltaY > 0 ? -0.1 : 0.1;
    setZoom((prev) => Math.min(Math.max(prev + delta, 0.2), 10));
  };

  const handleMouseDown = (e: React.MouseEvent) => {
    setDragging(true);
    dragStart.current = { x: e.clientX, y: e.clientY };
    dragOffset.current = { ...position };
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!dragging) return;
    const dx = e.clientX - dragStart.current.x;
    const dy = e.clientY - dragStart.current.y;
    setPosition({
      x: dragOffset.current.x + dx,
      y: dragOffset.current.y + dy,
    });
  };

  const handleMouseUp = () => setDragging(false);

  const zoomIn = () => setZoom((z) => Math.min(z + 0.2, 10));
  const zoomOut = () => setZoom((z) => Math.max(z - 0.2, 0.2));

  return (
    <div
      ref={containerRef}
      style={{ width: '100%', height: '100vh', overflow: 'hidden', position: 'relative' }}
    >
      {/* Zoom Controls */}
      <div style={{ position: 'absolute', top: 10, right: 10, zIndex: 2 }}>
        <button onClick={zoomIn} style={{ margin: '4px' }}>➕</button>
        <button onClick={zoomOut} style={{ margin: '4px' }}>➖</button>
      </div>

      {/* Zoomable & Draggable SVG Container */}
      <div
        onWheel={handleWheel}
        onMouseDown={handleMouseDown}
        onMouseMove={handleMouseMove}
        onMouseUp={handleMouseUp}
        onMouseLeave={handleMouseUp}
        style={{
          width: '100%',
          height: '100%',
          cursor: dragging ? 'grabbing' : 'grab',
        }}
      >
        <div
          ref={svgWrapperRef}
          style={{
            transform: `translate(${position.x}px, ${position.y}px) scale(${zoom})`,
            transformOrigin: '0 0',
            width: 'fit-content',
            height: 'fit-content',
          }}
          dangerouslySetInnerHTML={{ __html: svgContent }}
        />
      </div>
    </div>
  );
};

export default Map;
